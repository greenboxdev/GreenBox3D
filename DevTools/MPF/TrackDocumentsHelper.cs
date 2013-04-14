// TrackDocumentsHelper.cs
// 
// Copyright (c) 2013 The GreenBox Development LLC, all rights reserved
// 
// This file is a proprietary part of GreenBox3D, disclosing the content
// of this file without the owner consent may lead to legal actions

using System;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.Project
{
    /// <summary>
    ///     Used by a project to query the environment for permission to add, remove, or rename a file or directory in a solution
    /// </summary>
    internal class TrackDocumentsHelper
    {
        #region fields

        private readonly ProjectNode projectMgr;

        #endregion

        #region properties

        #endregion

        #region ctors

        internal TrackDocumentsHelper(ProjectNode project)
        {
            projectMgr = project;
        }

        #endregion

        #region helper methods

        /// <summary>
        ///     Gets the IVsTrackProjectDocuments2 object by asking the service provider for it.
        /// </summary>
        /// <returns>the IVsTrackProjectDocuments2 object</returns>
        private IVsTrackProjectDocuments2 GetIVsTrackProjectDocuments2()
        {
            Debug.Assert(projectMgr != null && !projectMgr.IsClosed && projectMgr.Site != null);

            IVsTrackProjectDocuments2 documentTracker =
                projectMgr.Site.GetService(typeof(SVsTrackProjectDocuments)) as IVsTrackProjectDocuments2;
            if (documentTracker == null)
            {
                throw new InvalidOperationException();
            }

            return documentTracker;
        }

        /// <summary>
        ///     Asks the environment for permission to add files.
        /// </summary>
        /// <param name="files">The files to add.</param>
        /// <param name="flags">The VSQUERYADDFILEFLAGS flags associated to the files added</param>
        /// <returns>true if the file can be added, false if not.</returns>
        internal bool CanAddItems(string[] files, VSQUERYADDFILEFLAGS[] flags)
        {
            // If we are silent then we assume that the file can be added, since we do not want to trigger this event.
            if ((projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
            {
                return true;
            }

            if (files == null || files.Length == 0)
            {
                return false;
            }

            int len = files.Length;
            VSQUERYADDFILERESULTS[] summary = new VSQUERYADDFILERESULTS[1];
            ErrorHandler.ThrowOnFailure(GetIVsTrackProjectDocuments2()
                                            .OnQueryAddFiles(projectMgr.InteropSafeIVsProject3, len, files, flags,
                                                             summary, null));
            if (summary[0] == VSQUERYADDFILERESULTS.VSQUERYADDFILERESULTS_AddNotOK)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Notify the environment about a file just added
        /// </summary>
        internal void OnItemAdded(string file, VSADDFILEFLAGS flag)
        {
            if ((projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
            {
                ErrorHandler.ThrowOnFailure(
                    GetIVsTrackProjectDocuments2()
                        .OnAfterAddFilesEx(projectMgr.InteropSafeIVsProject3, 1, new string[1] { file },
                                           new VSADDFILEFLAGS[1] { flag }));
            }
        }

        /// <summary>
        ///     Asks the environment for permission to remove files.
        /// </summary>
        /// <param name="files">an array of files to remove</param>
        /// <param name="flags">The VSQUERYREMOVEFILEFLAGS associated to the files to be removed.</param>
        /// <returns>true if the files can be removed, false if not.</returns>
        internal bool CanRemoveItems(string[] files, VSQUERYREMOVEFILEFLAGS[] flags)
        {
            // If we are silent then we assume that the file can be removed, since we do not want to trigger this event.
            if ((projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
            {
                return true;
            }

            if (files == null || files.Length == 0)
            {
                return false;
            }
            int length = files.Length;

            VSQUERYREMOVEFILERESULTS[] summary = new VSQUERYREMOVEFILERESULTS[1];

            ErrorHandler.ThrowOnFailure(
                GetIVsTrackProjectDocuments2()
                    .OnQueryRemoveFiles(projectMgr.InteropSafeIVsProject3, length, files, flags, summary, null));
            if (summary[0] == VSQUERYREMOVEFILERESULTS.VSQUERYREMOVEFILERESULTS_RemoveNotOK)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///     Notify the environment about a file just removed
        /// </summary>
        internal void OnItemRemoved(string file, VSREMOVEFILEFLAGS flag)
        {
            if ((projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
            {
                ErrorHandler.ThrowOnFailure(
                    GetIVsTrackProjectDocuments2()
                        .OnAfterRemoveFiles(projectMgr.InteropSafeIVsProject3, 1, new string[1] { file },
                                            new VSREMOVEFILEFLAGS[1] { flag }));
            }
        }

        /// <summary>
        ///     Asks the environment for permission to rename files.
        /// </summary>
        /// <param name="oldFileName">Path to the file to be renamed.</param>
        /// <param name="newFileName">Path to the new file.</param>
        /// <param name="flag">The VSRENAMEFILEFLAGS associated with the file to be renamed.</param>
        /// <returns>true if the file can be renamed. Otherwise false.</returns>
        internal bool CanRenameItem(string oldFileName, string newFileName, VSRENAMEFILEFLAGS flag)
        {
            // If we are silent then we assume that the file can be renamed, since we do not want to trigger this event.
            if ((projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) != 0)
            {
                return true;
            }

            int iCanContinue = 0;
            ErrorHandler.ThrowOnFailure(
                GetIVsTrackProjectDocuments2()
                    .OnQueryRenameFile(projectMgr.InteropSafeIVsProject3, oldFileName, newFileName, flag,
                                       out iCanContinue));
            return (iCanContinue != 0);
        }

        /// <summary>
        ///     Get's called to tell the env that a file was renamed
        /// </summary>
        internal void OnItemRenamed(string strOldName, string strNewName, VSRENAMEFILEFLAGS flag)
        {
            if ((projectMgr.EventTriggeringFlag & ProjectNode.EventTriggering.DoNotTriggerTrackerEvents) == 0)
            {
                ErrorHandler.ThrowOnFailure(
                    GetIVsTrackProjectDocuments2()
                        .OnAfterRenameFile(projectMgr.InteropSafeIVsProject3, strOldName, strNewName, flag));
            }
        }

        #endregion
    }
}
