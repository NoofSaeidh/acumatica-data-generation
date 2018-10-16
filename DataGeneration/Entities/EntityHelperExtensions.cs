using DataGeneration.Soap;
using System;

namespace DataGeneration.Entities
{
    public static class EntityHelperExtensions
    {
        public static Guid? GetNoteId(this Entity entity)
        {
            // adding interface for NoteID is not bad idea,
            // but it very time consuming to write each entity as partial and writing just interface to it
            // so let it be from this helper

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            switch (entity)
            {
                case INoteIdEntity noteIdEntity:
                    return noteIdEntity.NoteID;

                default:
                    try
                    {
                        return (EntityHelper.GetPropertyValue(entity, "NoteID") as GuidValue)?.Value;
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("Current entity doesn't contain NoteID field.", e);
                    }
            }
        }
    }
}