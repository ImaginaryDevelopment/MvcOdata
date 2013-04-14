
namespace Contracts
{
	using System;
	using System.Linq.Expressions;
	using System.Reflection;

		/// http://codebetter.com/patricksmacchia/2010/06/28/elegant-infoof-operators-in-c-read-info-of/
		public static class LinqOp
		{
			public static MethodInfo MethodOf<T>(Expression<Func<T>> expression)
			{
				var body = (MethodCallExpression)expression.Body;

				return body.Method;
			}

			public static MethodInfo MethodOf(Expression<Action> expression)
			{
				var body = (MethodCallExpression)expression.Body;

				return body.Method;
			}

			public static MethodInfo MethodOf<T>(Expression<Func<T, object>> expression)
			{
				var methExp = (MethodCallExpression)MaybeUnary(expression);

				return methExp.Method;
			}

			public static MethodInfo MethodOf<T>(Expression<Action<T>> expression)
			{

				var methExp = (MethodCallExpression)MaybeUnary(expression);

				return methExp.Method;
			}

			public static ConstructorInfo ConstructorOf<T>(Expression<Func<T>> expression)
			{
				var body = (NewExpression)expression.Body;

				return body.Constructor;
			}

			/// <summary>
			/// For getting a static property name, for instance properties use the other overload   
			/// </summary>
			public static PropertyInfo PropertyOf<T>(Expression<Func<T>> expression)
			{
				var body = (MemberExpression)expression.Body;

				return (PropertyInfo)body.Member;
			}

			static Expression MaybeUnary<T>(Expression<T> exp)
			{
				Expression result;
				var uExp = exp.Body as UnaryExpression;
				result = uExp != null ? uExp.Operand : exp.Body;
				return result;
			}
			/// <summary>
			/// Store a simple reference to your linqop delegate for reuse throughout the class or view
			/// </summary>
			/// <typeparam name="T"></typeparam>
			/// <returns></returns>
			public static Func<Expression<Func<T, object>>, string> PropertyNameHelper<T>()
			{
				return e => PropertyOf(e).Name;
			}

			public static PropertyInfo PropertyOf<T>(Expression<Func<T, object>> expression)
			{
				var memExp = (MemberExpression)MaybeUnary(expression);


				return (PropertyInfo)memExp.Member;
			}

			public static FieldInfo FieldOf<T>(Expression<Func<T>> expression)
			{
				var body = (MemberExpression)expression.Body;

				return (FieldInfo)body.Member;
			}
		}
	}
