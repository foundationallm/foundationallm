using Azure.Storage.Blobs.Models;
using FoundationaLLM.Common.Models.System.Interfaces;
using FoundationaLLM.Common.Security.Store;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoundationaLLM.Common.Security
{
    public class SecurityManager
    {
        static public ISecurityDatastore Datastore;
        static public ISecurityDatastore Principlestore;

        static public Principal User;

        static public bool IsAdmin;

        public void ValidateToken(string access_token)
        {
            //decode access token...
            var decodedToken = new JwtSecurityToken(jwtEncodedString: access_token);
            //var decodedToken = Jose.JWT.Payload(access_token);
        }

        public Principal CreatePrincipal(string upn, PrincipalType type)
        {
            //lookup from datastore...
            Principal p = new Principal();
            p.Id = "";
            p.Type = type;
            return p;
        }

        public Principal CreatePrincipalByAccessToken(string access_token)
        {
            //decode access token...
            var decodedToken = new JwtSecurityToken(jwtEncodedString: access_token);
            //var decodedToken = Jose.JWT.Payload(access_token);

            Principal p = new Principal();
            p.Id = decodedToken.Claims.First(c => c.Type == "expiry").Value;
            p.Type = PrincipalType.User;
            return p;
        }

        async static public Task<List<RoleAssignment>> GetAccess(string scope)
        {
            //get all role assignments for the scope.
            List<RoleAssignment> roleAssignments = await Datastore.GetScopeAssignments(scope);
            return roleAssignments;
        }

        static public List<RoleAssignment> GetAccess(PermissionedObject item)
        {
            if (HasAccess("system/*", User, Action.Read))
                return new List<RoleAssignment>();

            //get all role assignments for the scope.
            List<RoleAssignment> roleAssignments = new List<RoleAssignment>();

            if (HasAccess("system/*", User, Action.Read))
                return roleAssignments;

            return roleAssignments;
        }

        //look up if the principal has correct access...
        async static public Task<bool> HasAccess(string scope, Principal principal, Role role)
        {
            if (HasAccess("system/*", User, Action.Read))
                return false;

            //find all role assignements based on target scope...
            List<RoleAssignment> assignments = await GetAccess(scope);

            //check if user is directly assigned
            foreach (RoleAssignment ra in assignments)
            {
                if (ra.Principal.Type == PrincipalType.User) {
                    //check if user is in a group that is assigned.
                    if (ra.Principal.Id == principal.Id && ra.Role.Id == role.Id)
                    {
                        return true;
                    }
                }
                else if ( ra.Principal.Type == PrincipalType.Group)
                {
                    //check if they have the correct role...
                    Principlestore.GetGroup(ra.Principal.Id);

                    if (ra.Role == role && Principlestore.IsGroupMember(ra.Principal.Id, principal.Id)) 
                        return true;
                }
            }


            return false;
        }

        static public bool HasAccess(string scope, Principal principal, Action action)
        {
            //find all role assignements based on target scope...
            List<RoleAssignment> assignments = GetAccess(scope).Result;

            //check if user is directly assigned
            foreach (RoleAssignment ra in assignments)
            {
                if (ra.Principal.Type == PrincipalType.User)
                {
                    //check if user is in a group that is assigned.
                    if (ra.Principal.Id == principal.Id)
                    {
                        if (ra.Role.Actions.Contains(action) || ra.Role.Actions.Contains(Action.All))
                            return true;
                    }
                }
                else if (ra.Principal.Type == PrincipalType.Group)
                {
                    //check if they have the correct role...
                    Principlestore.GetGroup(ra.Principal.Id);

                    if (
                        (ra.Role.Actions.Contains(action) && Principlestore.IsGroupMember(ra.Principal.Id, principal.Id))
                        || ra.Role.Actions.Contains(Action.All)
                        )
                        return true;
                }
            }


            return false;
        }

        static public void AddRoleAssignment(RoleAssignment roleAssignment)
        {
            //check if the current user has access to create role  assignments...ALL or Write
            if (HasAccess("/system/*",User, Action.Write))
                Datastore.AddRoleAssignments(roleAssignment);
        }

        static public void DeleteRoleAssignment(RoleAssignment roleAssignment)
        {
            if (HasAccess("/system/*", User, Action.Write))
                Datastore.DeleteRoleAssignments(roleAssignment);
        }

        static public bool IsActive(Principal principalId)
        {
            return Principlestore.IsActive(principalId);
        }

        static public bool IsInGroup(string groupId, Principal principal)
        {
            return Principlestore.IsGroupMember(groupId, principal.Id);
        }
    }
}
