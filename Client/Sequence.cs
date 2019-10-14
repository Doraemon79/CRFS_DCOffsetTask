using System;
using Common;

namespace Client
{
    public class Sequence
    {
        private readonly Signal _iSignal;
        private readonly Signal _qSignal;

        public Sequence(Signal iSignal, Signal qSignal)
        {
            _iSignal = iSignal;
            _qSignal = qSignal;
        }

        public Item[] GetItems(int count)
        {
            Item[] res = new Item[count];

            for (int i = 0; i < res.Length; i++)
            {
                res[i].I = (Int16)_iSignal.NextValue();
                res[i].Q = (Int16)_qSignal.NextValue();
            }

            return res;
        }
    }
}
