using System;
using System.Collections;

namespace PhontyPlus.utils
{
    public class SimpleEnumerator : IEnumerable
    {
        public IEnumerator enumerator;
        public Action prefixAction = () => { };
        public Action postfixAction = () => { };
        public Action<object> preItemAction = (a) => { };
        public Action<object> postItemAction = (a) => { };
        public Func<object, object> itemAction = (a) => { return a; };
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator GetEnumerator()
        {
            prefixAction();
            while (enumerator.MoveNext())
            {
                var item = enumerator.Current;
                preItemAction(item);
                yield return itemAction(item);
                postItemAction(item);
            }
            postfixAction();
        }
    }
}
