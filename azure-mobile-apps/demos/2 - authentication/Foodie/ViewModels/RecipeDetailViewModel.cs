﻿using System;
using Xamarin.Forms;
using System.Windows.Input;
using Foodie.Abstractions;

namespace Foodie
{
	public class RecipeDetailViewModel : BaseRecipeViewModel
	{
		INavigation _nav;
		public RecipeDetailViewModel(SecureRecipe recipe, INavigation nav)
		{
			_nav = nav;

			Recipe = recipe;
			PopulateViewModelProps();

			// Subscribe to the recipe updated message
			MessagingCenter.Subscribe<EditRecipeViewModel>(this, RecipeSavedMessage, async (obj) =>
			{
				var dataSvc = DependencyService.Get<IAzureService>();
				var recipeTable = await dataSvc.GetTable<SecureRecipe>();

				Recipe = await recipeTable.GetSingle(this.Recipe.Id);

				PopulateViewModelProps();
			});

		}

		void PopulateViewModelProps()
		{
			RecipeName = Recipe.RecipeName;
			ImageName = Recipe.ImageName;
			PreparationTime = Recipe.PreparationTime;
			CookTime = Recipe.CookTime;
			NumberOfServings = Recipe.NumberOfServings;
			MealType = Recipe.MealType;
			Difficulty = Recipe.Difficulty;
			Ingredients = Recipe.Ingredients;
			Directions = Recipe.Directions;
		}

		SecureRecipe Recipe { get; set; }

		ICommand _showEditRecipe;
		public ICommand ShowEditRecipe 
		{
			get
			{
				if (_showEditRecipe == null)
				{
					_showEditRecipe= new Command(async () =>
					{
						await _nav.PushAsync(new EditRecipePage(Recipe));
					});
				}

				return _showEditRecipe;
			}
		}
	}
}
