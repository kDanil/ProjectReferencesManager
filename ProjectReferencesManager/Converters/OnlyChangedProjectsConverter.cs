﻿using ProjectReferencesManager.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace ProjectReferencesManager.Converters
{
    public class OnlyChangedProjectsConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var projects = value as IEnumerable<IProject>;
            if (projects == null)
            {
                throw new Exception("Wrong converter usage");
            }

            return projects.Where(p => p.IsChangedProject()).ToArray();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new OnlyChangedProjectsConverter();
        }
    }
}