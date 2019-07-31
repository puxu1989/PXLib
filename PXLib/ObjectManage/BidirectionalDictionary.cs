using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PXLib.ObjectManage
{
    /// <summary>
    /// 双向字典
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    public class BidirectionalDictionary<TFirst, TSecond>
    {
        private readonly IDictionary<TFirst, TSecond> _firstToSecond;

        private readonly IDictionary<TSecond, TFirst> _secondToFirst;

        private readonly string _duplicateFirstErrorMessage;

        private readonly string _duplicateSecondErrorMessage;

        public BidirectionalDictionary()
            : this(EqualityComparer<TFirst>.Default, EqualityComparer<TSecond>.Default)
        {
        }

        public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
            : this(firstEqualityComparer, secondEqualityComparer, "Duplicate item already exists for '{0}'.", "Duplicate item already exists for '{0}'.")
        {
        }

        public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer, string duplicateFirstErrorMessage, string duplicateSecondErrorMessage)
        {
            this._firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
            this._secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
            this._duplicateFirstErrorMessage = duplicateFirstErrorMessage;
            this._duplicateSecondErrorMessage = duplicateSecondErrorMessage;
        }

        public void Set(TFirst first, TSecond second)
        {
            TSecond tSecond;
            if (this._firstToSecond.TryGetValue(first, out tSecond) && !tSecond.Equals(second))
            {
                throw new ArgumentException(string.Format(this._duplicateFirstErrorMessage,first));
            }
            TFirst tFirst;
            if (this._secondToFirst.TryGetValue(second, out tFirst) && !tFirst.Equals(first))
            {
                throw new ArgumentException(string.Format(this._duplicateFirstErrorMessage,second));
            }
            this._firstToSecond.Add(first, second);
            this._secondToFirst.Add(second, first);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return this._firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return this._secondToFirst.TryGetValue(second, out first);
        }
    }
}
