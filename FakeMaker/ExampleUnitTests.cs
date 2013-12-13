﻿using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using NUnit.Framework;

namespace FakeMaker
{
	[TestFixture]
	public class ExampleUnitTests
	{
		private FakeMaker _fake;

		[SetUp]
		public void Setup()
		{
			_fake = new FakeMaker();
		}

		[Test]
		public void Get_descendants()
		{
			// Arrange
			var root = FakePage
				.Create("Root");

			var start = FakePage
				.Create("Start")
				.IsChildOf(root);

			FakePage
				.Create("About us")
				.IsChildOf(start);

			_fake.AddToRepository(root);

			// Act
			var children = ExampleFindPagesHelper.GetDescendantsOf(root.ContentLink, _fake.ContentRepository);

			//Assert
			Assert.That(children.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Get_children_of_first_child()
		{
			// Arrange
			var root = FakePage
				.Create("Root");

			FakePage
				.Create("my page")
				.IsChildOf(root);
			
			var start = FakePage
				.Create("Start")
				.IsChildOf(root);

			FakePage
				.Create("About us")
				.IsChildOf(start);

			FakePage
				.Create("Our services")
				.IsChildOf(start);

			_fake.AddToRepository(root);

			// Act
			var children = ExampleFindPagesHelper.GetDescendantsOf(start.ContentLink, _fake.ContentRepository);

			//Assert
			Assert.That(children.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Get_published_only_pages()
		{
			// Arrange
			var root = FakePage
				.Create("Root");

			var start = FakePage
				.Create("Start")
				.IsChildOf(root)
				.PublishedOn(DateTime.Now.AddDays(-10));

			FakePage
				.Create("About us")
				.IsChildOf(start)
				.PublishedOn(DateTime.Now.AddDays(-10), DateTime.Now.AddDays(-1));

			FakePage
				.Create("Our services")
				.IsChildOf(start)
				.PublishedOn(DateTime.Now.AddHours(-1));

			_fake.AddToRepository(root);

			// Act
			var children = ExampleFindPagesHelper.GetAllPublishedPages(root.ContentLink, _fake.ContentRepository);

			//Assert
			Assert.That(children.Count(), Is.EqualTo(2));
		}

		[Test]
		public void Get_pages_visible_in_menu()
		{
			// Arrange
			var root = FakePage.Create("root");

			FakePage.Create("AboutUs").IsChildOf(root).IsVisibleInMenu();
			FakePage.Create("OtherPage").IsChildOf(root).IsHiddenFromMenu();
			FakePage.Create("Contact").IsChildOf(root).IsVisibleInMenu();

			_fake.AddToRepository(root);

			// Act
			var pages = ExampleFindPagesHelper.GetMenu(root.ContentLink, _fake.ContentRepository);

			// Assert
			Assert.That(pages.Count(), Is.EqualTo(2));
		}
	}

	/// <summary>
	/// This is an example of a helper class.
	/// The repository is injected to the class (and/or methods).
	/// </summary>
	public static class ExampleFindPagesHelper
	{
		public static IEnumerable<IContent> GetChildrenOf(ContentReference root, IContentRepository repository)
		{
			return repository.GetChildren<IContent>(root);
		}

		public static IEnumerable<ContentReference> GetDescendantsOf(ContentReference root, IContentRepository repository)
		{
			return repository.GetDescendents(root);
		}

		public static IEnumerable<IContent> GetAllPublishedPages(ContentReference root, IContentRepository repository)
		{
			var descendants = GetDescendantsOf(root, repository);

			var references = descendants
				.Where(item => ToPage(item, repository).CheckPublishedStatus(PagePublishedStatus.Published));

			return references.Select(reference => ToPage(reference, repository)).Cast<IContent>().ToList();
		}

		private static PageData ToPage(ContentReference reference, IContentLoader repository)
		{
			var page = repository.Get<PageData>(reference);

			return page;
		}

		public static IEnumerable<IContent> GetMenu(ContentReference reference, IContentRepository repository)
		{
			var children = repository.GetChildren<PageData>(reference);

			return children.Where(page => page.VisibleInMenu).ToList();
		}
	}
}
