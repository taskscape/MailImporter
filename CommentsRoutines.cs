using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaskBeat.Logic.Comments;
using TaskBeat.Logic.MailImporter;
using TaskBeat.Logic.Profile;
using TaskBeat.Logic.Routines.Interfaces;

namespace MailImporter
{
    public class CommentsRoutines : ICommentsRoutines
    {
        // IQueryable<CommentDTO> GetAllByTask(int id, int context);
        int CreateCommentAndReturnId(CommentDTO comment) { throw new NotImplementedException(); }
        bool DeleteCommentByProfile(int commentId, int taskId, ProfileDTO profileObject) { throw new NotImplementedException(); }
        bool DeleteCommentByTask(int commentId, int taskId) { throw new NotImplementedException(); }
        // IQueryable<CommentDTO> GetAllByUser(int profileId);
        // bool ModifyComment(int id, int profileId, string newCommentContent);
        bool CreateComentByTaskReference(int refNumber, string sender, string commentBody, DateTime date, int ContextID)
        {
            using (FileStream file = new FileStream($"{ContextID}/{refNumber}.txt", FileMode.Append, FileAccess.Write))
            {
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.Write(commentBody);
                }
            }
            return true;
        }
        List<int> LikesForTask(int taskId, int profileId) { throw new NotImplementedException(); }
        bool CommentLike(int commentId, int taskId, int profileId, int contextId) { throw new NotImplementedException(); }
        bool CommentDislike(int commentId, int taskId, int profileId) { throw new NotImplementedException(); }
        bool RemindFollowers(CommentDTO noteStruct) { throw new NotImplementedException(); }
    }
}
