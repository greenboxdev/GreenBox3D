// OAAssemblyReference.cs
// 
// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
// 
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using VSLangProj;

namespace Microsoft.VisualStudio.Project.Automation
{
    [SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
    [ComVisible(true)]
    public class OAAssemblyReference : OAReferenceBase<AssemblyReferenceNode>
    {
        public OAAssemblyReference(AssemblyReferenceNode assemblyReference)
            :
                base(assemblyReference)
        {
        }

        #region Reference override

        public override int BuildNumber
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Build;
            }
        }

        public override string Culture
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.CultureInfo))
                {
                    return string.Empty;
                }
                return BaseReferenceNode.ResolvedAssembly.CultureInfo.Name;
            }
        }

        public override string Identity
        {
            get
            {
                // Note that in this function we use the assembly name instead of the resolved one
                // because the identity of this reference is the assembly name needed by the project,
                // not the specific instance found in this machine / environment.
                if (null == BaseReferenceNode.AssemblyName)
                {
                    return null;
                }
                return BaseReferenceNode.AssemblyName.FullName;
            }
        }

        public override int MajorVersion
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Major;
            }
        }

        public override int MinorVersion
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Minor;
            }
        }

        public override string PublicKeyToken
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.GetPublicKeyToken()))
                {
                    return null;
                }
                StringBuilder builder = new StringBuilder();
                byte[] publicKeyToken = BaseReferenceNode.ResolvedAssembly.GetPublicKeyToken();
                for (int i = 0; i < publicKeyToken.Length; i++)
                {
                    builder.AppendFormat("{0:x}", publicKeyToken[i]);
                }
                return builder.ToString();
            }
        }

        public override string Name
        {
            get
            {
                if (null != BaseReferenceNode.ResolvedAssembly)
                {
                    return BaseReferenceNode.ResolvedAssembly.Name;
                }
                if (null != BaseReferenceNode.AssemblyName)
                {
                    return BaseReferenceNode.AssemblyName.Name;
                }
                return null;
            }
        }

        public override int RevisionNumber
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return 0;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.Revision;
            }
        }

        public override bool StrongName
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (0 == (BaseReferenceNode.ResolvedAssembly.Flags & AssemblyNameFlags.PublicKey)))
                {
                    return false;
                }
                return true;
            }
        }

        public override prjReferenceType Type
        {
            get { return prjReferenceType.prjReferenceTypeAssembly; }
        }

        public override string Version
        {
            get
            {
                if ((null == BaseReferenceNode.ResolvedAssembly) ||
                    (null == BaseReferenceNode.ResolvedAssembly.Version))
                {
                    return string.Empty;
                }
                return BaseReferenceNode.ResolvedAssembly.Version.ToString();
            }
        }

        #endregion
    }
}
