using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tennis01.Rules
{
    struct MatchRule
    {
        public bool Advantage;
        public int Games;
        public int Sets;
        public bool TieBreak;
        public MatchRule(int games,int sets,bool advantage,bool tieBreak)
        {
            this.Games = games;
            this.Sets = sets;
            this.Advantage = advantage;
            TieBreak = tieBreak;
        }
        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            int set = Sets + (Sets - 1);
            if (Games == 0)
            {
                s.Append("Tie Break Match");
                if (Sets > 1)
                {
                    s.Append("(").Append(set).Append(")");
                }
                return s.ToString();
            }
            s.Append(Games).Append("Game ").Append(set).Append("Set Match").Append('\n');
            if (!Advantage || !TieBreak)
            {
                s.Append("(");
                if (!Advantage)
                {
                    s.Append("No Adv");
                    if (!TieBreak)
                    {
                        s.Append(", ");
                    }
                }
                if (!TieBreak)
                {
                    s.Append("No Tie");
                }
                s.Append(")");
            }
            return s.ToString();

        }
    }
}
