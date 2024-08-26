using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.IO;
using OpenCiv.Engine;
using OpenCiv.Presentation.Media.Imaging;

namespace OpenCiv.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DependencyObject> _hitResultsList = new List<DependencyObject>();

        private Engine.Engine Engine
        {
            get
            {
                return this.DataContext as OpenCiv.Engine.Engine;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Engine.NodeUpdate += Engine_NodeUpdate;
        }

        private void Engine_NodeUpdate(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => { })).Wait();
        }

        private void Grid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Engine.IsProcessing || Engine.IsProcessingTurn) return;

            var selectedUnit = Engine.SelectedUnit;
            var UIElement = Mouse.DirectlyOver as UIElement;
            Point pt = e.GetPosition((UIElement)sender);

            // Clear the contents of the list used for hit test results.
            _hitResultsList.Clear();

            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(this,
                              new HitTestFilterCallback(MyHitTestFilter),
                              new HitTestResultCallback(MyHitTestResult),
                              new PointHitTestParameters(pt));

            // Perform actions on the hit test results list.
            if (_hitResultsList.Count > 0)
            {
                foreach(var result in _hitResultsList)
                {
                    Image image = result as Image;
                    if (image != null && image.DataContext != null)
                    {
                        Tile tile = image.DataContext as Tile;
                        if (tile != null)
                        {
                            if (tile.HasUnit && tile.CurrentUnit.Owner == Engine.PlayerCivilization) return;

                            e.Handled = true;
                            Engine.TryMoveSelectedUnit(tile);

                            break;
                        }
                    }
                }
            }
        }

        public HitTestFilterBehavior MyHitTestFilter(DependencyObject o)
        {
            // Test for the object value you want to filter.
            if (o.GetType() != typeof(Image))
            {
                // Visual object and descendants are NOT part of hit test results enumeration.
                return HitTestFilterBehavior.ContinueSkipSelf;
            }
            else
            {
                // Visual object is part of hit test results enumeration.
                return HitTestFilterBehavior.Continue;
            }
        }

        // Return the result of the hit test to the callback.
        public HitTestResultBehavior MyHitTestResult(HitTestResult result)
        {
            // Add the hit test result to the list that will be processed after the enumeration.
            _hitResultsList.Add(result.VisualHit);

            // Set the behavior to return visuals at all z-order levels.
            return HitTestResultBehavior.Continue;
        }

        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!Engine.IsInRangedMode) return;
            if (Engine.IsProcessing || Engine.IsProcessingTurn) return;

            var selectedUnit = Engine.SelectedUnit;
            var UIElement = Mouse.DirectlyOver as UIElement;
            Point pt = e.GetPosition((UIElement)sender);

            // Clear the contents of the list used for hit test results.
            _hitResultsList.Clear();

            // Set up a callback to receive the hit test result enumeration.
            VisualTreeHelper.HitTest(this,
                              new HitTestFilterCallback(MyHitTestFilter),
                              new HitTestResultCallback(MyHitTestResult),
                              new PointHitTestParameters(pt));

            // Perform actions on the hit test results list.
            if (_hitResultsList.Count > 0)
            {
                foreach (var result in _hitResultsList)
                {
                    Image image = result as Image;
                    if (image != null && image.DataContext != null)
                    {
                        Tile tile = image.DataContext as Tile;
                        if (tile != null)
                        {
                            Engine.TryRangedAttackWithSelectedUnit(tile);
                            e.Handled = true;
                            break;
                        }
                    }
                }
            }
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Engine.IsInRangedMode)
            {
                Engine.ExitRangedMode();
            }
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(Engine.IsProcessing || Engine.IsProcessingTurn || Engine.SelectedUnit == null))
            {

                var selectedUnit = Engine.SelectedUnit;
                var UIElement = Mouse.DirectlyOver as UIElement;
                Point pt = e.GetPosition((UIElement)sender);

                // Clear the contents of the list used for hit test results.
                _hitResultsList.Clear();

                // Set up a callback to receive the hit test result enumeration.
                VisualTreeHelper.HitTest(this,
                                  new HitTestFilterCallback(MyHitTestFilter),
                                  new HitTestResultCallback(MyHitTestResult),
                                  new PointHitTestParameters(pt));

                // Perform actions on the hit test results list.
                if (_hitResultsList.Count > 0)
                {
                    foreach (var result in _hitResultsList)
                    {
                        Image image = result as Image;
                        if (image != null && image.DataContext != null)
                        {
                            Tile tile = image.DataContext as Tile;
                            if (tile != null && tile.HasUnit && tile.CurrentUnit.Owner != selectedUnit.Owner)
                            {
                                Engine.SetEnemyDataContext(tile.CurrentUnit);
                                e.Handled = true;
                                return;
                            }
                        }
                    }
                }

                Engine.SetEnemyDataContext(null);
            }
        }
    }
}
