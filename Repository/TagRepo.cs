using DAO;
using Models;
using System;
using System.Collections.Generic;

namespace Repository
{
    public class TagRepo : ITagRepo
    {
        public IEnumerable<Tag> GetAllTags() =>
            TagDAO.Instance.GetAllTags();

        public Tag GetTagById(int id) =>
            TagDAO.Instance.GetTagById(id);

        public Tag GetTagByName(string name) =>
            TagDAO.Instance.GetTagByName(name);

        public IEnumerable<Tag> SearchTags(string searchTerm) =>
            TagDAO.Instance.SearchTags(searchTerm);

        public void AddTag(Tag tag) =>
            TagDAO.Instance.AddTag(tag);

        public void UpdateTag(Tag tag) =>
            TagDAO.Instance.UpdateTag(tag);

        public void DeleteTag(int id) =>
            TagDAO.Instance.DeleteTag(id);

        public IEnumerable<Tag> GetTagsByArticleId(int articleId) =>
            TagDAO.Instance.GetTagsByArticleId(articleId);

        public bool IsTagUsedInArticles(int tagId) =>
            TagDAO.Instance.IsTagUsedInArticles(tagId);
    }
}