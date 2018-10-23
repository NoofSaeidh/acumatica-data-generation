using DataGeneration.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Common
{
    public interface IRelation<TLeft, TRight> 
    {
        IEnumerable<TLeft> Left { get; }
        IEnumerable<TRight> Right { get; }
    }

    public class OneToManyRelation<TLeft, TRight> : IRelation<TLeft, TRight>
    {
        private readonly ReadOnlyCollection<TLeft> _left;
        private readonly IEnumerable<TRight> _right;

        public OneToManyRelation(TLeft left, params TRight[] right) : this(left, (IEnumerable<TRight>)right)
        {
        }

        public OneToManyRelation(TLeft left, IEnumerable<TRight> right)
        {
            _right = right ?? throw new ArgumentNullException(nameof(right));
            _left = new ReadOnlyCollection<TLeft>(new TLeft[] { Left = left });
        }


        public TLeft Left { get; }
        public IEnumerable<TRight> Right => _right;

        IEnumerable<TLeft> IRelation<TLeft, TRight>.Left => _left;
    }
}
