using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TaskBeat.Logic.TaskReport;
using TaskBeat.Logic.Tasks;
using TaskBeat.Logic.Profile;
using TaskBeat.Logic.TimeLine;
using TaskBeat.Logic.Routines.Interfaces;

namespace MailImporter
{
    public class TaskRoutines : ITaskRoutines
    {
        IQueryable<TaskDTO> GetAll(ProfileDTO profile, int parentId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTasksAllFlat(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTasksCurrentProfile(ProfileDTO profile, int? parentId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTasksForTimeline(string profileName) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTasksFlatAllOrdered(ProfileDTO profile) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTasksFlatAllOrderedExceptDeleted(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllUnassignedTasks(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllOverDueTasks(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllTodaysTasks(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllMyTasks(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllMyTodaysTasks(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllMyOverdueTasks(ProfileDTO profile, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllMyFocusTasks(ProfileDTO profile, IEnumerable<TimeLineTaskDto> reportTasks) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllMyFocusTasks(ProfileDTO profile) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetAllTasksAssignedToSpecificUser(ProfileDTO profile, string user, bool hideCompleted) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTaskByReference(int refNumber, int? contextId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTaskByClientName(string clientName, int? contextId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTaskByClientEmail(string clientEmail, int? contextId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTaskByUserName(string userName, int? contextId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTaskByTag(string tagText, int? contextId) { throw new NotImplementedException(); }
        IQueryable<TaskReportDto> GetHolidaysForContext(int contextId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetTasksVisibleOnCalendar(int contextId) { throw new NotImplementedException(); }
        List<TaskDTO> GetTasksForPriorityUpdate(int profileContext, int parentTask, int currentProgress) { throw new NotImplementedException(); }
        List<SharedTaskDTO> ReturnSharedTask(int taskId) { throw new NotImplementedException(); }
        bool IsProjectLocked(int topProjectId, int sourceTaskId) { throw new NotImplementedException(); }
        List<TaskStateDto> GetTaskStates(int contextId) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> GetProjects(int topProjectId) { throw new NotImplementedException(); }

        bool GetTopTask(ProfileDTO profile) { throw new NotImplementedException(); }
        int GetTopTaskId(int contextId) { throw new NotImplementedException(); }
        List<TaskDTO> GetTaskParents(ProfileDTO profile, int p) { throw new NotImplementedException(); }
        TaskDTO GetTaskById(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool GetTaskBelongsToProfile(ProfileDTO profile, int? taskId) { throw new NotImplementedException(); }
        TaskDTO GetTaskOnCurrentLevel(ProfileDTO profile, int id, int parent) { throw new NotImplementedException(); }
        TaskDTO ReturnTaskObjectByGuid(string guid) { throw new NotImplementedException(); }

        DebugTaskDTO GetTaskLevelByTitle(string taskTitle) { throw new NotImplementedException(); }
        List<string> GetTagsFromTask(int taskId) { throw new NotImplementedException(); }
        string GetTaskUrl(string taskReference, int contextId, string baseUrl = "") { throw new NotImplementedException(); }
        TaskDTO SetTaskParentTasks(ProfileDTO profile, TaskDTO task, bool skipRoot, bool skipLast) { throw new NotImplementedException(); }
        TaskDTO SetTaskSubTasks(ProfileDTO profile, TaskDTO task) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> SetTasksParentTasks(ProfileDTO profile, List<TaskDTO> tasks) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> SetTasksSubTasks(ProfileDTO profile, List<TaskDTO> tasks) { throw new NotImplementedException(); }
        IQueryable<TaskDTO> SetTaskStates(IQueryable<TaskDTO> tasks, List<TaskStateDto> states) { throw new NotImplementedException(); }

        bool CanModifyTask(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool CanModifyTask(ProfileDTO profile, TaskDTO task) { throw new NotImplementedException(); }
        bool CanModifyTask(ProfileDTO profile, TaskDTO oldTask, TaskDTO newTask) { throw new NotImplementedException(); }
        bool CanDeleteTask(ProfileDTO profile, TaskDTO task) { throw new NotImplementedException(); }

        int CreateTopTask(ProfileDTO profile, string topTask) { throw new NotImplementedException(); }
        TaskDTO CreateTask(TaskDTO newTask, ProfileDTO profile) { throw new NotImplementedException(); }
        int CreateTask(string newTitle, ProfileDTO profile) { throw new NotImplementedException(); }
        void CreateTaskByEmail(TaskDTO task) { throw new NotImplementedException(); }
        int CreateTaskWithNotes(string taskTitle, string taskNotes, string clientName, string clientEmail, ProfileDTO profile)
        {
            Directory.CreateDirectory(profile.Context.Value.ToString());
            using (FileStream file = new FileStream($"{profile.Context.Value}/{taskTitle}.txt", FileMode.Create, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.Write(taskNotes);
                }
            }
            return 0;
        }
        TaskDTO DuplicateProject(int sourceTaskId, ProfileDTO profileObject, string duplicatedString) { throw new NotImplementedException(); }

        bool UpdateTask(ProfileDTO profile, TaskDTO taskObject, TaskDTO taskUpdated, bool simpleUpdate = false) { throw new NotImplementedException(); }
        //void UpdateAllTasks(ProfileDTO Profile, List<TaskDTO> tasks);
        void UpdateTaskPriorities(ProfileDTO profile, List<TaskDTO> tasks) { throw new NotImplementedException(); }
        bool MarkAsPrivate(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool MarkAsOnCalendar(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool MarkAsOnChart(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool Subscribe(int profileid, int taskid, bool subscribe) { throw new NotImplementedException(); }
        bool MoveTask(int subtreeId, int parentId, int contextId, bool directionUp, ProfileDTO profile) { throw new NotImplementedException(); }
        bool RemoveTask(TaskDTO task, ProfileDTO profile) { throw new NotImplementedException(); }
        bool UpdateTasksProgress(int taskId, int? parentTaskId) { throw new NotImplementedException(); }
        bool RemoveSharedTask(int linkId, string taskGuid, ProfileDTO profileObject) { throw new NotImplementedException(); }
        bool CreateSharedTask(ProfileDTO profile, string profileName, int taskId) { throw new NotImplementedException(); }
        bool TaskLinkExists(string profileGuid, string taskGuid) { throw new NotImplementedException(); }
        bool SetFinishDate(int taskId, DateTime? finishDate) { throw new NotImplementedException(); }
        bool UpdateTaskColumnPriorities(int profileContext, TaskDTO updatedTask, int oldProgress, int taskPosition, int? oldTaskPosition) { throw new NotImplementedException(); }
        bool UpdateTaskModifiedDate(int taskId) { throw new NotImplementedException(); }

        bool InvalidateSubTasksCache(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool InvalidateParentTasksCache(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool InvalidateTaskCache(ProfileDTO profile, int taskId) { throw new NotImplementedException(); }
        bool InvalidateTaskCache(ProfileDTO profile) { throw new NotImplementedException(); }
    }
}
