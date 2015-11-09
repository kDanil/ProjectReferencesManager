using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ProjectReferencesManager
{
    /// <summary>
    /// Extension to raise notification without using a name of property.
    /// </summary>
    public static class INotifyPropertyChangedExtensions
    {
        public static string PropertyName<TProperty>(this Expression<Func<TProperty>> projection)
        {
            var body = projection.Body as MemberExpression;
            if (body == null)
            {
                throw new ArgumentException("'propertyExpression' should be a member expression");
            }

            return body.Member.Name;
        }

        public static void Raise<T>(this PropertyChangedEventHandler handler, Expression<Func<T>> propertyExpression)
        {
            if (handler != null)
            {
                var body = propertyExpression.Body as MemberExpression;
                if (body == null)
                {
                    throw new ArgumentException("'propertyExpression' should be a member expression");
                }

                var expression = body.Expression as ConstantExpression;
                if (expression == null)
                {
                    throw new ArgumentException("'propertyExpression' body should be a constant expression");
                }

                object target = Expression.Lambda(expression).Compile().DynamicInvoke();

                var e = new PropertyChangedEventArgs(body.Member.Name);
                handler(target, e);
            }
        }
    }
}