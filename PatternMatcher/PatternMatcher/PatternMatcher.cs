using System;
using System.Collections.Generic;
using Optional;

namespace PatternMatcher
{

    public class PatternMatcher<TOutput>
    {
        List<Tuple<Predicate<object>, Func<object, TOutput>>> cases = new List<Tuple<Predicate<object>, Func<object, TOutput>>>();
        private bool _includeSubtypes;

        public PatternMatcher() { }

        public PatternMatcher<TOutput> Case(Predicate<object> condition, Func<object, TOutput> function)
        {
            cases.Add(new Tuple<Predicate<object>, Func<object, TOutput>>(condition, function));
            return this;
        }

        public PatternMatcher<TOutput> Case<T>(Predicate<T> condition, Func<T, TOutput> function)
        {
            return Case(
                o => o is T && condition((T)o),
                o => function((T)o));
        }

        public PatternMatcher<TOutput> Case<T>(Func<T, TOutput> function)
        {
            return Case(
                o => o is T,
                o => function((T)o));
        }

        public PatternMatcher<TOutput> TypeCase<T>(Func<T, TOutput> function)
        {
            return Case(
                o => (Type)o == typeof(T) || (_includeSubtypes && ((Type)o).IsAssignableFrom(typeof(T))),
                o => function(default(T)));
        }

        public PatternMatcher<TOutput> IncludeSubtypes()
        {
            this._includeSubtypes = true;
            return this;
        }
        public PatternMatcher<TOutput> Case<T>(Predicate<T> condition, TOutput o)
        {
            return Case(condition, x => o);
        }

        public PatternMatcher<TOutput> Case<T>(TOutput o)
        {
            return Case<T>(x => o);
        }

        public PatternMatcher<TOutput> Default(Func<object, TOutput> function)
        {
            return Case(o => true, function);
        }

        public PatternMatcher<TOutput> Default(TOutput o)
        {
            return Default(x => o);
        }

        public Option<TOutput,Exception> Match(object o)
        {
            foreach (var tuple in cases)
                if (tuple.Item1(o))
                {
                    try
                    {
                        var tupleItem2 = tuple.Item2(o);
                        return tupleItem2.Some<TOutput,Exception>();
                    }
                    catch (Exception e)
                    {
                        return Option.None<TOutput>().WithException(e);
                    }
                }

            return Option.None<TOutput>().WithException(new Exception("Element Not Found"));
        }
    }
}