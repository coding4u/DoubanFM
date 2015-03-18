﻿using DoubanFM.Desktop.Account.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DoubanFM.Desktop.Account.Views
{
	/// <summary>
	/// Interaction logic for LoginView.xaml
	/// </summary>
	public partial class AccountLoginView : UserControl
	{
		public AccountLoginView()
		{
			InitializeComponent();
		}

		public AccountLoginView(AccountLoginViewModel viewModel)
			: this()
		{
			this.DataContext = viewModel;
		}
	}
}
