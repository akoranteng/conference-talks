﻿using Foodie.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
namespace Foodie
{
	public class RecipeListViewModel : BaseViewModel
	{
		IAzureService _dataSvc;
		public RecipeListViewModel()
		{
			InitializeAllRecipesGrouped();

			_dataSvc = DependencyService.Get<IAzureService>(DependencyFetchTarget.GlobalInstance);

			// Listen to save message
			MessagingCenter.Subscribe<EditRecipeViewModel>(this, RecipeSavedMessage, async (obj) => await ReadRecipeData());
		}

		void InitializeAllRecipesGrouped()
		{
			if (_allRecipesGrouped == null)
			{
				var easyGrouping = new ListViewGrouping<SecureRecipe>("Easy", "E");
				var mediumGrouping = new ListViewGrouping<SecureRecipe>("Medium", "M");
				var hardGrouping = new ListViewGrouping<SecureRecipe>("Hard", "H");

				// Add the groupings to the list view
				_allRecipesGrouped = new ObservableCollection<ListViewGrouping<SecureRecipe>> {
						easyGrouping,
						mediumGrouping,
						hardGrouping
					};
			}
		}

		ObservableCollection<ListViewGrouping<SecureRecipe>> _allRecipesGrouped;
		public ObservableCollection<ListViewGrouping<SecureRecipe>> AllRecipes
		{
			get
			{
				return _allRecipesGrouped;
			}
		}

		public async Task ReadRecipeData()
		{
			var recipeTable = await _dataSvc.GetTable<SecureRecipe>();
			var recipes = await recipeTable.GetAll();

			// iOS doesn't like things to be added on anything but the main thread
			Device.BeginInvokeOnMainThread(() =>
			{
				var easyGrouping = AllRecipes.First((arg) => arg.Title == Difficulty.Easy);
				var medGrouping = AllRecipes.First((arg) => arg.Title == Difficulty.Medium);
				var hardGrouping = AllRecipes.First((arg) => arg.Title == Difficulty.Hard);

				// This goes through and does a wholesale clean of the groupings
				easyGrouping.Clear();
				medGrouping.Clear();
				hardGrouping.Clear();

				// Then adds everything back in
				foreach (var item in recipes)
				{
					if (item.Difficulty == Difficulty.Easy)
						easyGrouping.Add(item);
					else if (item.Difficulty == Difficulty.Medium)
						medGrouping.Add(item);
					else if (item.Difficulty == Difficulty.Hard)
						hardGrouping.Add(item);
				}
			});
		}

		ICommand _addCommand;
		public ICommand AddRecipeCommand
		{
			get
			{
				if (_addCommand == null)
				{
					_addCommand = new Command(async (obj) => await App.Current.MainPage.Navigation.PushModalAsync(
						new NavigationPage(new EditRecipePage())));

				}
				return _addCommand;
			}
		}

		ICommand _pushRecipes;
		public ICommand PushRecipes
		{
			get
			{
				if (_pushRecipes == null)
				{
					_pushRecipes = new Command(async () =>
					{
						await _dataSvc.SyncOfflineCache();
						await ReadRecipeData();
					});
				}

				return _pushRecipes;
			}
		}
	}
}
