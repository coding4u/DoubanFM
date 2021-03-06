﻿using DoubanFM.Desktop.Infrastructure;
using Prism.Commands;
using Prism.Events;

namespace DoubanFM.Desktop.Search.ViewModels
{
    public class SearchViewModel : ViewModelBase
    {
        private bool _isSearching;
        private IEventAggregator _eventAggregator;

        public SearchViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            this.StartSearchCommand = new DelegateCommand(Search);
        }

        public DelegateCommand StartSearchCommand { get; set; }

        public bool IsSearching
        {
            get { return _isSearching; }
            private set
            {
                if (value != _isSearching)
                {
                    _isSearching = value;
                    OnPropertyChanged(() => this.IsSearching);
                }
            }
        }
        private void Search()
        {
            //throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
