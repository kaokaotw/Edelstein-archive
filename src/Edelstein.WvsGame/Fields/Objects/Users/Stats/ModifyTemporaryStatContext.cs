using System;
using System.Collections.Generic;
using System.Linq;
using Edelstein.Network.Packets;
using MoreLinq.Extensions;

namespace Edelstein.WvsGame.Fields.Objects.Users.Stats
{
    public class ModifyTemporaryStatContext
    {
        private readonly FieldUser _user;
        public readonly List<TemporaryStatEntry> ResetOperations;
        public readonly List<TemporaryStatEntry> SetOperations;

        public ModifyTemporaryStatContext(FieldUser user)
        {
            _user = user;
            ResetOperations = new List<TemporaryStatEntry>();
            SetOperations = new List<TemporaryStatEntry>();
        }

        public void Set(TemporaryStatType type, int templateID, short option)
        {
            var ts = new TemporaryStatEntry
            {
                Type = type,
                TemplateID = templateID,
                Option = option,
                Permanent = true
            };

            Reset(type);
            SetOperations.Add(ts);
            _user.TemporaryStat.Entries.Add(ts.Type, ts);
        }

        public void Set(TemporaryStatType type, int templateID, short option, DateTime dateExpire)
        {
            var ts = new TemporaryStatEntry
            {
                Type = type,
                TemplateID = templateID,
                Option = option,
                DateExpire = dateExpire
            };

            Reset(type);
            SetOperations.Add(ts);
            _user.TemporaryStat.Entries.Add(ts.Type, ts);
        }

        public void Reset(TemporaryStatType type)
        {
            if (_user.TemporaryStat.Entries.ContainsKey(type))
                ResetOperations.Add(_user.TemporaryStat.Entries[type]);
            _user.TemporaryStat.Entries.Remove(type);
        }
    }
}