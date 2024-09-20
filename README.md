I have added AuthServer, AdministrationService and IdentityService in this repo

SETUP :
Database changes
1. You need to create these tables in Identity database
   
   CREATE TABLE `TenantRolesAssociation` (
  `Id` char(36) NOT NULL,
  `RoleId` char(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
  `TenantId` char(36) DEFAULT NULL,
  `IsDefault` tinyint(1) NOT NULL,
  `IsPublic` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `RoleId` (`RoleId`),
  CONSTRAINT `TenantRolesAssociation_ibfk_1` FOREIGN KEY (`RoleId`) REFERENCES `AbpRoles` (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci


CREATE TABLE `user_tenant_association` (
  `Id` char(36) NOT NULL,
  `UserId` char(36) NOT NULL,
  `TenantId` char(36) DEFAULT NULL,
  `IsActive` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci


2. Now, you can create any tenant in your application (let's say tenant name is "test")
3. For this tenant, you will have an admin user, take the Id of that User from AbpUsers.
4. Take the Id of the Role "admin" which has TenantId as NULL (the admin of the host tenant).
5. In AbpUserRoles table, where you have the UserId of the admin user of your tenant, there in that dataset, you will have RoleId
   as the Id of the Role from AbpRoles table with the TenantId of your tenant, you have to replace this RoleId with the Role Id of
   the admin role from the Host tenant (which has Tenant Id as NULL in AbpRoles table)
6. The idea is we want to use the same roles across all the Tenants.
7. Now, the tables that we have created, TenantRolesAssociation and user_tenant_association, here you have to insert the dataset as:
   For user_tenant_association (Id: New Guid(), UserId: Id of User from AbpUsers, TenantId: Id of Tenant, IsActive: true)
   For TenantRolesAssociation (Id: New Guid(), RoleId: Id of Role from Host from AbpRoles, TenantId: Id of Tenant, IsDefault: true,
   IsPublic: true)
8. Now, whatever tenant you have given name, you have to put testdev.localhost as the URL of the AuthServer, and now you have to login
   and the login should be completed and it should redirect to the angular app with all the required roles and grantedPolicies.


