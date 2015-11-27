using AutoMapper.QueryableExtensions.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace AutoMapper.CoreCLR.Internal
{
    public static class ExpressionExtensions
    {

        public static void ForEach(this Expression subject, Action<Expression> action) 
        {
            var crawler = new ExpressionCrawler(action);
            crawler.Visit(subject);
        }
        

        public static IEnumerable<Expression> Find(this Expression subject, Predicate<Expression> test) 
        {
            var found = new List<Expression>();

            subject.ForEach(ex => {
                if(test(ex)) found.Add(ex);
            });

            return found;
        }
        


        public static Expression Replace(this Expression subject, Expression old, Expression replacement) 
        {
            return new ExpressionReplacer(ex => ex == old, _ => replacement)
                            .Visit(subject);
        }

        public static Expression Replace(this Expression subject, Predicate<Expression> test, Expression replacement) 
        {
            return new ExpressionReplacer(test, _ => replacement)
                            .Visit(subject);
        }

        public static Expression Replace(this Expression subject, Predicate<Expression> test, Func<Expression, Expression> replacer) 
        {
            return new ExpressionReplacer(test, replacer)
                            .Visit(subject);
        }


        public static Expression ReplaceParametersWith(this Expression subject, Expression replacement) 
        {
            return subject.Replace(ex => ex is ParameterExpression, replacement);
        }







        class ExpressionCrawler : ExpressionVisitor
        {
            Action<Expression> _action;

            public ExpressionCrawler(Action<Expression> action) {
                _action = action;
            }

            public override Expression Visit(Expression node) {
                _action(node);
                return base.Visit(node);
            }
        }

        class ExpressionReplacer : ExpressionVisitor
        {
            Predicate<Expression> _test;
            Func<Expression, Expression> _replacer;

            public ExpressionReplacer(Predicate<Expression> test, Func<Expression, Expression> replacer) {
                _test = test;
                _replacer = replacer;
            }

            public override Expression Visit(Expression node) {
                return _test(node)
                            ? _replacer(node)
                            : base.Visit(node);
            }

        }              


    }
}
