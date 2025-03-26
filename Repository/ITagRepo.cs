using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public interface ITagRepo
    {
        IEnumerable<Tag> GetAllTags();
        Tag GetTagById(int id);
        Tag GetTagByName(string name);
        IEnumerable<Tag> SearchTags(string searchTerm);
        void AddTag(Tag tag);
        void UpdateTag(Tag tag);
        void DeleteTag(int id);
        IEnumerable<Tag> GetTagsByArticleId(int articleId);
        bool IsTagUsedInArticles(int tagId);
    }
}