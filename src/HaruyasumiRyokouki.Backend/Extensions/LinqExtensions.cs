using HaruyasumiRyokouki.Backend.Models.Db;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace HaruyasumiRyokouki.Backend.Extensions;

/// <summary>
/// Query helpers for localized includes.
/// </summary>
public static class LinqExtensions
{
	private static Expression<Func<TSource, IEnumerable<TTarget>>> ApplyWhere<TSource, TTarget>
	(
		Expression<Func<TSource, IEnumerable<TTarget>>> navigation,
		Expression<Func<TTarget, bool>> predicate
	)
	{
		var parameter = Expression.Parameter(
			typeof(TTarget),
			predicate.Parameters[0].Name);

		var predicateBody = new ReplaceExpressionVisitor(
				predicate.Parameters[0],
				parameter)
			.Visit(predicate.Body);

		var newPredicate = Expression.Lambda<Func<TTarget, bool>>(
			predicateBody,
			parameter);

		var whereCall = Expression.Call(
			typeof(Enumerable),
			nameof(Enumerable.Where),
			new[] { typeof(TTarget) },
			navigation.Body,
			newPredicate);

		return Expression.Lambda<Func<TSource, IEnumerable<TTarget>>>(
			whereCall,
			navigation.Parameters);
	}

	/// <summary>
	/// <c>Include</c> of a collection navigation filtered by <paramref name="predicate"/>, e.g. only the translations in one language.
	/// </summary>
	public static IIncludableQueryable<TSource, IEnumerable<TTarget>> IncludeFiltered<TSource, TTarget>
	(
		this IQueryable<TSource> query,
		Expression<Func<TSource, IEnumerable<TTarget>>> navigation,
		Expression<Func<TTarget, bool>> predicate
	)
		where TSource : class
	{
		var filteredNavigation = ApplyWhere(
			navigation,
			predicate);

		return query.Include(filteredNavigation);
	}

	/// <summary>
	/// <c>ThenInclude</c> of a collection navigation filtered by <paramref name="predicate"/>.
	/// </summary>
	public static IIncludableQueryable<TSource, IEnumerable<TTarget>> ThenIncludeFiltered<TSource, TPrevious, TTarget>
	(
		this IIncludableQueryable<TSource, IEnumerable<TPrevious>> query,
		Expression<Func<TPrevious, IEnumerable<TTarget>>> navigation,
		Expression<Func<TTarget, bool>> predicate
	)
		where TSource : class
	{
		var filteredNavigation = ApplyWhere(
			navigation,
			predicate);

		return query.ThenInclude(filteredNavigation);
	}

	/// <summary>
	/// Day translations in the language.
	/// </summary>
	public static Expression<Func<DayTranslation, bool>> LocalizedDays(this string languageCode)
		=> x => x.LanguageCode == languageCode;

	/// <summary>
	/// Media translations in the language.
	/// </summary>
	public static Expression<Func<MediaTranslation, bool>> LocalizedMedia(this string languageCode)
		=> x => x.LanguageCode == languageCode;

	/// <summary>
	/// Display names of tags in the language; aliases are excluded.
	/// </summary>
	public static Expression<Func<TagTranslation, bool>> LocalizedTags(this string languageCode)
		=> t => t.LanguageCode == languageCode && t.IsPrimary;

	/// <summary>
	/// Display names of a tag, optionally only in one language.
	/// </summary>
	public static IEnumerable<TagTranslation> Primary(this IEnumerable<TagTranslation> query, string? acceptLanguage = null)
	{
		if (acceptLanguage != null)
			return query.Where(t => t.IsPrimary && t.LanguageCode == acceptLanguage);
		return query.Where(t => t.IsPrimary);
	}

	/// <summary>
	/// Search aliases of a tag, optionally only in one language.
	/// </summary>
	public static IEnumerable<TagTranslation> Aliases(this IEnumerable<TagTranslation> query, string? acceptLanguage = null)
	{
		if (acceptLanguage != null)
			return query.Where(t => !t.IsPrimary && t.LanguageCode == acceptLanguage);
		return query.Where(t => !t.IsPrimary);
	}
}

/// <summary>
/// Replaces one expression node with another; used to combine lambda expressions.
/// </summary>
public sealed class ReplaceExpressionVisitor : ExpressionVisitor
{
	private readonly Expression _from;
	private readonly Expression _to;

	/// <summary>
	/// Creates a visitor replacing <paramref name="from"/> with <paramref name="to"/>.
	/// </summary>
	public ReplaceExpressionVisitor(
		Expression from,
		Expression to)
	{
		_from = from;
		_to = to;
	}

	/// <inheritdoc/>
	public override Expression Visit(Expression? node)
	{
		if (node == _from)
			return _to;

		return base.Visit(node)!;
	}
}
