using DataGeneration.Soap;
using System;

namespace DataGeneration.Entities
{
    public static class KeyFields
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
                case Lead lead:
                    return lead.NoteID?.Value;

                //todo: extend endpoint
                //case Opportunity opportunity:
                //    return opportunity.NoteID?.Value;
                //case Case @case:
                //    return @case.NoteID?.Value;

                default:
                    try
                    {
                        return EntityHelper.GetPropertyValue(entity, "NoteID") as Guid?;
                    }
                    catch (Exception e)
                    {
                        throw new InvalidOperationException("Current entity doesn't contain NoteID field.", e);
                    }
            }
        }

        public static string GetNaturalKey(this Entity entity)
        {
            //todo: perhaps may be needed
            throw new NotImplementedException();
        }
    }
}