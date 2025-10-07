using AutoMapper;
using IAMLegend.Data;
using IAMLegend.Dtos;
using IAMLegend.Entities;
using IAMLegend.Models;
using IAMLegend.Repositories;
using IAMLegend.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IAMLegend.Services
{
    public class UserService : IUserService
    {
        private readonly IQueryUserRepository _query;
        private readonly ICommandUserRepository _command;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public UserService(IQueryUserRepository query, ICommandUserRepository command, ApplicationDbContext db, IMapper mapper)
        {
            _query = query;
            _command = command;
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllAsync(CancellationToken ct = default)
        {
            var users = await _query.GetAllUsersAsync(ct);
            return users.Select(u => _mapper.Map<UserDto>(u)).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            var user = await _query.GetUserByIdAsync(id, ct);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<int> CreateAsync(CreateUserRequest req, string createdBy, CancellationToken ct = default)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(req.domain) ||
                string.IsNullOrWhiteSpace(req.firstname) ||
                string.IsNullOrWhiteSpace(req.lastname))
                throw new ArgumentException("Domain, FirstName and LastName are required");

            // Ensure unique username/email
            var exists = await _query.GetUserByDomainUsernameAsync(req.domain, req.username, ct);
            if (exists != null) throw new InvalidOperationException("Username already exists");

            var emailConflict = (await _query.GetAllUsersAsync(ct)).Any(u => u.email == req.email);
            if (emailConflict) throw new InvalidOperationException("Email already exists");

            // Business validations: branches require permission and vice versa
            foreach (var s in req.SystemPermissions)
            {
                var hasBranchChecked = s.Branches?.Any(b => b.IsSelected) ?? false;
                if (hasBranchChecked && s.PermissionLevelId <= 0)
                    throw new InvalidOperationException($"System {s.LocalSystemId}: Permission required when branch selected");

                if (s.PermissionLevelId > 0 && !hasBranchChecked)
                    throw new InvalidOperationException($"System {s.LocalSystemId}: select at least one branch for selected permission");
            }

            // Transaction: create profile and associated entries
            using var trx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                var user = new UserProfile
                {
                    domain = req.domain.Trim(),
                    firstname = req.firstname.Trim(),
                    lastname = req.lastname.Trim(),
                    username = req.username.Trim(),
                    email = req.email.Trim(),
                    isadmin = req.isadmin,
                    status = 0,
                    createddatetime = DateTime.UtcNow
                };

                await _command.InsertUserProfileAsync(user, ct);

                // insert branches and access
                foreach (var s in req.SystemPermissions)
                {
                    foreach (var b in s.Branches ?? Enumerable.Empty<CreateUserRequest.BranchSelection>())
                    {
                        if (b.IsSelected)
                        {
                            await _command.InsertUserSystemBranchAsync(new UserSystemBranch
                            {
                                userprofileid = user.userprofileid,
                                localsystemid = s.LocalSystemId,
                                branchcode = b.BranchCode.Trim(),
                                status = 0
                            }, ct);
                        }
                    }
                    if (s.PermissionLevelId > 0)
                    {
                        await _command.InsertUserSystemAccessAsync(new UserSystemAccess
                        {
                            userprofileid = user.userprofileid,
                            localsystemid = s.LocalSystemId,
                            permissionlevelid = s.PermissionLevelId,
                            status = 0
                        }, ct);
                    }
                }

                await _command.SaveChangesAsync(ct);
                await trx.CommitAsync(ct);
                return user.userprofileid;
            }
            catch
            {
                await trx.RollbackAsync(ct);
                throw;
            }
        }

        public async Task UpdateAsync(int id, UpdateUserRequest req, string modifiedBy, CancellationToken ct = default)
        {
            var user = await _db.UserProfiles.Include(u => u.UserSystemAccesses).Include(u => u.UserSystemBranches)
                                             .FirstOrDefaultAsync(u => u.userprofileid == id && u.status >= 0, ct);
            if (user == null) throw new KeyNotFoundException("User not found");

            // Business validation same as create
            foreach (var s in req.SystemPermissions)
            {
                var hasBranchChecked = s.Branches?.Any(b => b.IsSelected) ?? false;
                if (hasBranchChecked && s.PermissionLevelId <= 0)
                    throw new InvalidOperationException($"System {s.LocalSystemId}: Permission required when branch selected");
                if (s.PermissionLevelId > 0 && !hasBranchChecked)
                    throw new InvalidOperationException($"System {s.LocalSystemId}: select at least one branch for selected permission");
            }

            using var trx = await _db.Database.BeginTransactionAsync(ct);
            try
            {
                // update allowed fields
                user.isadmin = req.IsAdmin;
                user.createddatetime = DateTime.UtcNow;

                // Soft delete old ones
                await _command.SoftDeleteUserSystemBranchesAsync(id, modifiedBy, ct);
                await _command.SoftDeleteUserSystemAccessAsync(id, modifiedBy, ct);

                await _command.SaveChangesAsync(ct);

                // Insert new ones
                foreach (var s in req.SystemPermissions)
                {
                    foreach (var b in s.Branches ?? Enumerable.Empty<UpdateUserRequest.BranchSelection>())
                    {
                        if (b.IsSelected)
                        {
                            await _command.InsertUserSystemBranchAsync(new UserSystemBranch
                            {
                                userprofileid = id,
                                localsystemid = s.LocalSystemId,
                                branchcode = b.BranchCode.Trim(),
                                status = 0
                            }, ct);
                        }
                    }
                    if (s.PermissionLevelId > 0)
                    {
                        await _command.InsertUserSystemAccessAsync(new UserSystemAccess
                        {
                            userprofileid = id,
                            localsystemid = s.LocalSystemId,
                            permissionlevelid = s.PermissionLevelId,
                            status = 0
                        }, ct);
                    }
                }

                await _command.SaveChangesAsync(ct);
                await trx.CommitAsync(ct);
            }
            catch
            {
                await trx.RollbackAsync(ct);
                throw;
            }
        }        

        public async Task SoftDeleteUserProfileAsync(int userId, string modifiedBy, CancellationToken ct = default)
        {
            var user = await _db.UserProfiles.FindAsync(new object[] { userId }, ct);
            if (user == null) return;

            user.status = -1;
            user.createddatetime = DateTime.UtcNow;

            // Soft delete related entities
            var branches = await _db.UserSystemBranches
                .Where(b => b.userprofileid == userId && b.status >= 0)
                .ToListAsync(ct);
            branches.ForEach(b => b.status = -1);

            var accesses = await _db.UserSystemAccesses
                .Where(a => a.userprofileid == userId && a.status >= 0)
                .ToListAsync(ct);
            accesses.ForEach(a => a.status = -1);

            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateUserPermissionsAsync(UserProfileEditViewModel model, string modifiedBy, CancellationToken ct = default)
        {
            // Fetch existing user
            var user = await _db.UserProfiles
                .Include(u => u.UserSystemAccesses)
                .Include(u => u.UserSystemBranches)
                .FirstOrDefaultAsync(u => u.userprofileid == model.UserProfile.userprofileid, ct);

            if (user == null) throw new KeyNotFoundException($"UserProfile with ID {model.UserProfile.userprofileid} not found.");

            // Soft delete old access and branch records
            foreach (var access in user.UserSystemAccesses.Where(a => a.status >= 0)) access.status = -1;

            foreach (var branch in user.UserSystemBranches.Where(b => b.status >= 0)) branch.status = -1;

            await _db.SaveChangesAsync(ct); // commit soft deletes first

            // Detach old entities so EF doesn't track them anymore
            foreach (var access in user.UserSystemAccesses.ToList())
                _db.Entry(access).State = EntityState.Detached;

            foreach (var branch in user.UserSystemBranches.ToList())
                _db.Entry(branch).State = EntityState.Detached;

            // Insert new access and branch rows
            foreach (var row in model.PermissionsRows)
            {
                // Save Access if selected
                if (row.SelectedPermissionLevelId > 0)
                {
                    var newAccess = new UserSystemAccess
                    {
                        userprofileid = user.userprofileid,
                        localsystemid = row.LocalSystemId,
                        permissionlevelid = row.SelectedPermissionLevelId,
                        status = 0,
                        createdby = modifiedBy,
                        createddatetime = DateTime.UtcNow
                    };
                    _db.UserSystemAccesses.Add(newAccess);
                }

                // Save checked branches
                foreach (var kv in row.Branches.Where(b => b.Value.IsChecked))
                {
                    var newBranch = new UserSystemBranch
                    {
                        userprofileid = user.userprofileid,
                        localsystemid = row.LocalSystemId,
                        branchcode = kv.Key,
                        status = 0,
                        createdby = modifiedBy,
                        createddatetime = DateTime.UtcNow
                    };
                    _db.UserSystemBranches.Add(newBranch);
                }
            }

            // Update basic user fields
            user.username = model.UserProfile.username;
            user.firstname = model.UserProfile.firstname;
            user.lastname = model.UserProfile.lastname;
            user.email = model.UserProfile.email;
            user.domain = model.UserProfile.domain;
            user.isadmin = model.UserProfile.isadmin;
            user.modifiedby = modifiedBy;
            user.modifieddatetime = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }


        //public async Task UpdateUserPermissionsAsync(UserProfileEditViewModel model, string modifiedBy, CancellationToken ct = default)
        //{
        //    // Fetch existing user from DB
        //    var user = await _db.UserProfiles.FindAsync(new object[] { model.UserProfile.userprofileid }, ct);
        //    if (user == null) throw new KeyNotFoundException($"UserProfile with ID {model.UserProfile.userprofileid} not found.");

        //    // Soft delete old rows
        //    var oldAccess = _db.UserSystemAccesses.Where(a => a.userprofileid == model.UserProfile.userprofileid && a.status >= 0);
        //    await oldAccess.ForEachAsync(a => a.status = -1, ct);

        //    var oldBranches = _db.UserSystemBranches.Where(b => b.userprofileid == model.UserProfile.userprofileid && b.status >= 0);
        //    await oldBranches.ForEachAsync(b => b.status = -1, ct);

        //    await _db.SaveChangesAsync(ct);

        //    // Insert new rows
        //    foreach (var row in model.PermissionsRows)
        //    {
        //        // Save Access
        //        var access = new UserSystemAccess
        //        {
        //            status = 0,
        //            userprofileid = model.UserProfile.userprofileid,
        //            localsystemid = row.LocalSystemId,
        //            permissionlevelid = row.SelectedPermissionLevelId,
        //            createddatetime = DateTime.UtcNow,
        //            createdby = modifiedBy
        //        };
        //        _db.UserSystemAccesses.Add(access);

        //        // Save Branches
        //        foreach (var kv in row.Branches.Where(b => b.Value)) // only checked
        //        {
        //            var branch = new UserSystemBranch
        //            {
        //                status = 0,
        //                userprofileid = model.UserProfile.userprofileid,
        //                localsystemid = row.LocalSystemId,
        //                branchcode = kv.Key,
        //                createddatetime = DateTime.UtcNow,
        //                createdby = modifiedBy
        //            };
        //            _db.UserSystemBranches.Add(branch);
        //        }
        //    }

        //    // Update fields
        //    user.username = model.UserProfile.username;
        //    user.firstname = model.UserProfile.firstname;
        //    user.lastname = model.UserProfile.lastname;
        //    user.email = model.UserProfile.email;
        //    user.domain = model.UserProfile.domain;
        //    user.isadmin = model.UserProfile.isadmin;            
        //    user.modifiedby = modifiedBy;
        //    user.modifieddatetime = DateTime.UtcNow; // track last modified

        //    await _db.SaveChangesAsync(ct);
        //}

        //public async Task<UserProfileEditViewModel> GetUserProfileWithPermissionsAsync(int userId, CancellationToken ct = default)
        //{
        //    var user = await _db.UserProfiles
        //        .FirstOrDefaultAsync(u => u.userprofileid == userId && u.status >= 0, ct);

        //    if (user == null) return null;

        //    // load all systems
        //    var systems = await _db.LocalSystems.ToListAsync(ct);
        //    var branches = await _db.Branches.ToListAsync(ct);
        //    var access = await _db.UserSystemAccesses.Where(a => a.userprofileid == userId && a.status >= 0).ToListAsync(ct);
        //    var userBranches = await _db.UserSystemBranches.Where(b => b.userprofileid == userId && b.status >= 0).ToListAsync(ct);
        //    var permissionLevels = await _db.PermissionLevels.ToListAsync(ct);

        //    var rows = systems.Select(s => new PermissionRowViewModel
        //    {
        //        LocalSystemId = s.localsystemid,
        //        LocalSystemName = s.localsystemname,
        //        Branches = branches.ToDictionary(
        //            br => br.branchcode.Trim(),
        //            br => userBranches.Any(ub => ub.localsystemid == s.localsystemid && ub.branchcode == br.branchcode && ub.status >= 0)
        //        ),
        //        PermissionLevelList = permissionLevels
        //            .Select(pl => new SelectListItem
        //            {
        //                Value = pl.permissionlevelid.ToString(),
        //                Text = pl.name
        //            })
        //            .ToList(),
        //        SelectedPermissionLevelId = access.FirstOrDefault(a => a.localsystemid == s.localsystemid)?.permissionlevelid ?? 0
        //    }).ToList();

        //    return new UserProfileEditViewModel
        //    {
        //        UserProfile = user, 
        //        PermissionsRows = rows                
        //    };
        //}

        // inside your UserService class

        public async Task<UserProfileEditViewModel> GetUserProfileWithPermissionsAsync(int userId, CancellationToken ct = default)
        {
            // load user
            var user = await _db.UserProfiles.FirstOrDefaultAsync(u => u.userprofileid == userId && u.status >= 0, ct);

            if (user == null) throw new KeyNotFoundException($"User {userId} not found.");            

            // preload all data we need (no await inside LINQ Select)
            var systems = await _db.LocalSystems.OrderBy(s => s.localsystemname).ToListAsync(ct);
            var branches = await _db.Branches.OrderBy(b => b.branchname).ToListAsync(ct);
            var accessList = await _db.UserSystemAccesses
                .Where(a => a.userprofileid == userId && a.status >= 0)
                .ToListAsync(ct);
            var userBranches = await _db.UserSystemBranches
                .Where(b => b.userprofileid == userId && b.status >= 0)
                .ToListAsync(ct);
            var permissionLevels = await _db.PermissionLevels
               /* .Where(pl => pl.status >= 0) */  // optional filter if you track status on permission levels
                .ToListAsync(ct);

            // build fast lookup structures
            var accessLookup = accessList
                .GroupBy(a => a.localsystemid)
                .ToDictionary(g => g.Key, g => g.First());

            var userBranchesLookup = userBranches
                .GroupBy(b => b.localsystemid)
                .ToDictionary(g => g.Key, g => new HashSet<string>(g.Select(x => x.branchcode.Trim()), StringComparer.OrdinalIgnoreCase));

            var permissionLevelsLookup = permissionLevels
                .GroupBy(pl => pl.localsystemid)
                .ToDictionary(g => g.Key, g => g.ToList());

            // build rows
            var rows = systems.Select(s =>
            {
                // Branch dictionary: all branches (keys), checked if user has an active usersystembranch entry
                //var branchDict = branches.ToDictionary(
                //    br => br.branchcode.Trim(),
                //    br => userBranches.Any(ub => ub.localsystemid == s.localsystemid
                //                 && ub.branchcode.Trim() == br.branchcode.Trim()
                //                 && ub.status >= 0));

                //var branchDict = branches.ToDictionary(
                //    br => br.branchcode.Trim(),
                //    br => new {
                //        br.branchname, // full name like "London"
                //        IsChecked = userBranches.Any(ub =>
                //            ub.localsystemid == s.localsystemid &&
                //            ub.branchcode.Trim() == br.branchcode.Trim() &&
                //            ub.status >= 0)
                //    });

                //var branchDict = branches.ToDictionary(
                //    br => br.branchcode.Trim(),
                //    br => (BranchName: br.branchname,
                //            IsChecked: userBranches.Any(ub =>
                //                ub.localsystemid == s.localsystemid &&
                //                ub.branchcode.Trim() == br.branchcode.Trim() &&
                //                ub.status >= 0))
                //    );

                var branchDict = branches.ToDictionary(
                   br => br.branchcode.Trim(),
                   br => new BranchCheck
                   {
                       BranchName = br.branchname, // full name
                       IsChecked = userBranches.Any(ub =>
                           ub.localsystemid == s.localsystemid &&
                           ub.branchcode.Trim() == br.branchcode.Trim() &&
                           ub.status >= 0)
                   });


                List<SelectListItem> permissionSelectList = new();
                if (permissionLevelsLookup.TryGetValue(s.localsystemid, out var pls))
                {
                    permissionSelectList = pls
                        .Select(pl => new SelectListItem
                        {
                            Value = pl.permissionlevelid.ToString(),
                            Text = pl.name
                        })
                        .ToList();
                }

                // selected permission id (from usersystemaccess) if exists
                var selectedPermissionId = accessLookup.TryGetValue(s.localsystemid, out var acc)
                    ? acc.permissionlevelid
                    : 0;

                return new PermissionRowViewModel
                {
                    LocalSystemId = s.localsystemid,
                    LocalSystemName = s.localsystemname,
                    Branches = branchDict,
                    PermissionLevelList = permissionSelectList,
                    SelectedPermissionLevelId = selectedPermissionId
                };
            }).ToList();

            
            // wrap into edit VM (make sure property names match your actual UserProfileEditViewModel)
            return new UserProfileEditViewModel
            {
                UserProfile = user,
                PermissionsRows = rows
            };
        }

        public async Task CreateUserAsync(UserProfileEditViewModel model, string createdBy, CancellationToken ct = default)
        {
            var newUser = new UserProfile
            {
                username = model.UserProfile.username,
                firstname = model.UserProfile.firstname,
                lastname = model.UserProfile.lastname,
                email = model.UserProfile.email,
                domain = model.UserProfile.domain,
                isadmin = model.UserProfile.isadmin,
                status = 0,
                createdby = createdBy,
                createddatetime = DateTime.UtcNow
            };

            // Add access/branch records if chosen
            foreach (var row in model.PermissionsRows)
            {
                if (row.SelectedPermissionLevelId > 0)
                {
                    newUser.UserSystemAccesses.Add(new UserSystemAccess
                    {
                        localsystemid = row.LocalSystemId,
                        permissionlevelid = row.SelectedPermissionLevelId,
                        status = 0,
                        createdby = createdBy,
                        createddatetime = DateTime.UtcNow
                    });
                }

                foreach (var kv in row.Branches)
                {
                    if (kv.Value != null && kv.Value.IsChecked) // only save checked
                    {
                        newUser.UserSystemBranches.Add(new UserSystemBranch
                        {
                            localsystemid = row.LocalSystemId,
                            branchcode = kv.Key,
                            status = 0,
                            createdby = createdBy,
                            createddatetime = DateTime.UtcNow
                        });
                    }
                }

                //foreach (var kv in row.Branches.Where(b => b.Value.IsChecked))
                //{
                //    newUser.UserSystemBranches.Add(new UserSystemBranch
                //    {
                //        localsystemid = row.LocalSystemId,
                //        branchcode = kv.Key,
                //        status = 0,
                //        createdby = createdBy,
                //        createddatetime = DateTime.UtcNow
                //    });
                //}
            }

            _db.UserProfiles.Add(newUser);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<UserProfileEditViewModel> GetBlankUserProfileWithPermissionsAsync(CancellationToken ct = default)
        {
            // Get all systems
            var systems = await _db.LocalSystems
                .Include(s => s.PermissionLevels)
                .OrderBy(s => s.localsystemname)   // ensure alphabetical
                .ToListAsync(ct);

            // Get branches
            var branches = await _db.Branches.ToListAsync(ct);

            var rows = systems.Select(s => new PermissionRowViewModel
            {
                LocalSystemId = s.localsystemid,
                LocalSystemName = s.localsystemname,
                SelectedPermissionLevelId = 0,
                PermissionLevelList = s.PermissionLevels
                    .Select(p => new SelectListItem { Value = p.permissionlevelid.ToString(), Text = p.name })
                    .ToList(),                
                Branches = branches.ToDictionary(b => b.branchcode.Trim(), b => new BranchCheck { BranchName = b.branchname, IsChecked = false })
            }).ToList();

            return new UserProfileEditViewModel
            {
                UserProfile = new UserProfile(), // empty profile for form
                PermissionsRows = rows
            };
        }

    }
}