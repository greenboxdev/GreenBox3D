// OAProjectItem.cs
// 
// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
// 
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project.Automation
{
    [ComVisible(true), CLSCompliant(false)]
    public class OAProjectItem<T> : ProjectItem
        where T : HierarchyNode
    {
        #region fields

        private readonly T node;
        private readonly OAProject project;

        #endregion

        #region properties

        protected T Node
        {
            get { return node; }
        }

        /// <summary>
        ///     Returns the automation project
        /// </summary>
        protected OAProject Project
        {
            get { return project; }
        }

        #endregion

        #region ctors

        public OAProjectItem(OAProject project, T node)
        {
            this.node = node;
            this.project = project;
        }

        #endregion

        #region EnvDTE.ProjectItem

        /// <summary>
        ///     Gets the requested Extender if it is available for this object
        /// </summary>
        /// <param name="extenderName">The name of the extender.</param>
        /// <returns>The extender object.</returns>
        public virtual object get_Extender(string extenderName)
        {
            return null;
        }

        /// <summary>
        ///     Gets an object that can be accessed by name at run time.
        /// </summary>
        public virtual object Object
        {
            get { return node.Object; }
        }

        /// <summary>
        ///     Gets the Document associated with the item, if one exists.
        /// </summary>
        public virtual Document Document
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets the number of files associated with a ProjectItem.
        /// </summary>
        public virtual short FileCount
        {
            get { return 1; }
        }

        /// <summary>
        ///     Gets a collection of all properties that pertain to the object.
        /// </summary>
        public virtual Properties Properties
        {
            get
            {
                return UIThread.DoOnUIThread(delegate
                {
                    if (node.NodeProperties == null)
                    {
                        return null;
                    }
                    return new OAProperties(node.NodeProperties);
                });
            }
        }

        /// <summary>
        ///     Gets the FileCodeModel object for the project item.
        /// </summary>
        public virtual FileCodeModel FileCodeModel
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets a ProjectItems for the object.
        /// </summary>
        public virtual ProjectItems ProjectItems
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets a GUID string indicating the kind or type of the object.
        /// </summary>
        public virtual string Kind
        {
            get
            {
                Guid guid;
                ErrorHandler.ThrowOnFailure(node.GetGuidProperty((int)__VSHPROPID.VSHPROPID_TypeGuid, out guid));
                return guid.ToString("B");
            }
        }

        /// <summary>
        ///     Saves the project item.
        /// </summary>
        /// <param name="fileName">The name with which to save the project or project item.</param>
        /// <remarks>Implemented by subclasses.</remarks>
        public virtual void Save(string fileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the top-level extensibility object.
        /// </summary>
        public virtual DTE DTE
        {
            get { return project.DTE; }
        }

        /// <summary>
        ///     Gets the ProjectItems collection containing the ProjectItem object supporting this property.
        /// </summary>
        public virtual ProjectItems Collection
        {
            get
            {
                return UIThread.DoOnUIThread(delegate
                {
                    // Get the parent node
                    HierarchyNode parentNode = node.Parent;
                    Debug.Assert(parentNode != null, "Failed to get the parent node");

                    // Get the ProjectItems object for the parent node
                    if (parentNode is ProjectNode)
                    {
                        // The root node for the project
                        return ((OAProject)parentNode.GetAutomationObject()).ProjectItems;
                    }
                    else if (parentNode is FileNode && parentNode.FirstChild != null)
                    {
                        // The item has children
                        return ((OAProjectItem<FileNode>)parentNode.GetAutomationObject()).ProjectItems;
                    }
                    else if (parentNode is FolderNode)
                    {
                        return ((OAProjectItem<FolderNode>)parentNode.GetAutomationObject()).ProjectItems;
                    }
                    else
                    {
                        // Not supported. Override this method in derived classes to return appropriate collection object
                        throw new NotImplementedException();
                    }
                });
            }
        }

        /// <summary>
        ///     Gets a list of available Extenders for the object.
        /// </summary>
        public virtual object ExtenderNames
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets the ConfigurationManager object for this ProjectItem.
        /// </summary>
        /// <remarks>We do not support config management based per item.</remarks>
        public virtual ConfigurationManager ConfigurationManager
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets the project hosting the ProjectItem.
        /// </summary>
        public virtual EnvDTE.Project ContainingProject
        {
            get { return project; }
        }

        /// <summary>
        ///     Gets or sets a value indicating whether or not the object has been modified since last being saved or opened.
        /// </summary>
        public virtual bool Saved
        {
            get { return !IsDirty; }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Gets the Extender category ID (CATID) for the object.
        /// </summary>
        public virtual string ExtenderCATID
        {
            get { return null; }
        }

        /// <summary>
        ///     If the project item is the root of a subproject, then the SubProject property returns the Project object for the subproject.
        /// </summary>
        public virtual EnvDTE.Project SubProject
        {
            get { return null; }
        }

        /// <summary>
        ///     Microsoft Internal Use Only. Checks if the document associated to this item is dirty.
        /// </summary>
        public virtual bool IsDirty
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        ///     Gets or sets the name of the object.
        /// </summary>
        public virtual string Name
        {
            get { return node.Caption; }
            set
            {
                if (node == null || node.ProjectMgr == null || node.ProjectMgr.IsClosed || node.ProjectMgr.Site == null)
                {
                    throw new InvalidOperationException();
                }

                UIThread.DoOnUIThread(delegate
                {
                    using (AutomationScope scope = new AutomationScope(Node.ProjectMgr.Site))
                    {
                        node.SetEditLabel(value);
                    }
                });
            }
        }

        /// <summary>
        ///     Removes the project item from hierarchy.
        /// </summary>
        public virtual void Remove()
        {
            if (node == null || node.ProjectMgr == null || node.ProjectMgr.IsClosed || node.ProjectMgr.Site == null)
            {
                throw new InvalidOperationException();
            }

            UIThread.DoOnUIThread(delegate
            {
                using (AutomationScope scope = new AutomationScope(Node.ProjectMgr.Site))
                {
                    node.Remove(false);
                }
            });
        }

        /// <summary>
        ///     Removes the item from its project and its storage.
        /// </summary>
        public virtual void Delete()
        {
            if (node == null || node.ProjectMgr == null || node.ProjectMgr.IsClosed || node.ProjectMgr.Site == null)
            {
                throw new InvalidOperationException();
            }

            UIThread.DoOnUIThread(delegate
            {
                using (AutomationScope scope = new AutomationScope(Node.ProjectMgr.Site))
                {
                    node.Remove(true);
                }
            });
        }

        /// <summary>
        ///     Saves the project item.
        /// </summary>
        /// <param name="newFileName">The file name with which to save the solution, project, or project item. If the file exists, it is overwritten.</param>
        /// <returns>true if save was successful</returns>
        /// <remarks>This method is implemented on subclasses.</remarks>
        public virtual bool SaveAs(string newFileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets a value indicating whether the project item is open in a particular view type.
        /// </summary>
        /// <param name="viewKind">
        ///     A Constants.vsViewKind* indicating the type of view to check./param>
        ///     <returns>A Boolean value indicating true if the project is open in the given view type; false if not. </returns>
        public virtual bool get_IsOpen(string viewKind)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Gets the full path and names of the files associated with a project item.
        /// </summary>
        /// <param name="index"> The index of the item</param>
        /// <returns>The full path of the associated item</returns>
        /// <exception cref="ArgumentOutOfRangeException">Is thrown if index is not one</exception>
        public virtual string get_FileNames(short index)
        {
            // This method should really only be called with 1 as the parameter, but
            // there used to be a bug in VB/C# that would work with 0. To avoid breaking
            // existing automation they are still accepting 0. To be compatible with them
            // we accept it as well.
            Debug.Assert(index > 0, "Index is 1 based.");
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index");
            }
            return node.Url;
        }

        /// <summary>
        ///     Expands the view of Solution Explorer to show project items.
        /// </summary>
        public virtual void ExpandView()
        {
            if (node == null || node.ProjectMgr == null || node.ProjectMgr.IsClosed || node.ProjectMgr.Site == null)
            {
                throw new InvalidOperationException();
            }

            UIThread.DoOnUIThread(delegate
            {
                using (AutomationScope scope = new AutomationScope(Node.ProjectMgr.Site))
                {
                    IVsUIHierarchyWindow uiHierarchy = UIHierarchyUtilities.GetUIHierarchyWindow(node.ProjectMgr.Site,
                                                                                                 HierarchyNode
                                                                                                     .SolutionExplorer);
                    if (uiHierarchy == null)
                    {
                        throw new InvalidOperationException();
                    }

                    ErrorHandler.ThrowOnFailure(uiHierarchy.ExpandItem(node.ProjectMgr.InteropSafeIVsUIHierarchy,
                                                                       node.ID, EXPANDFLAGS.EXPF_ExpandFolder));
                }
            });
        }

        /// <summary>
        ///     Opens the project item in the specified view. Not implemented because this abstract class dont know what to open
        /// </summary>
        /// <param name="ViewKind">Specifies the view kind in which to open the item</param>
        /// <returns>Window object</returns>
        public virtual Window Open(string ViewKind)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
