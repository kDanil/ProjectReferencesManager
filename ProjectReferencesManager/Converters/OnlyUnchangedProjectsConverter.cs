using ProjectReferencesManager.Model;
using ProjectReferencesManager.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Markup;

namespace ProjectReferencesManager.Converters
{
    public class OnlyUnchangedProjectsConverter : MarkupExtension, IValueConverter
    {
        public OnlyUnchangedProjectsConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var projects = value as IEnumerable<IProject>;
            if (projects == null)
            {
                throw new Exception("Wrong converter usage");
            }

            return new ProjectCollectionsModifier().Prepare(projects.Where(p => !p.IsChangedProject()));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new OnlyUnchangedProjectsConverter();
        }
    }
}