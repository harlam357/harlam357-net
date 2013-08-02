/*
 * harlam357.Net - Sortable Binding List
 * Copyright (C) 2010-2013 Ryan Harlamert (harlam357)
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

/* 
 * Implementation by Joe Stegman - Microsoft Corporation
 * http://social.msdn.microsoft.com/forums/en-US/winformsdatacontrols/thread/12eb59d3-e687-4e36-93ab-bf6741954d39/
 * 
 * IBindingListView, Sorted Event, and plugable SortComparer<T> implementation by harlam357
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Windows.Forms;

namespace harlam357.Windows.Forms
{
   /// <summary>
   /// Provides a generic collection that supports data binding, sorting, and filtering.
   /// </summary>
   /// <typeparam name="T">The type of elements in the list.</typeparam>
   public class SortableBindingList<T> : BindingList<T>, IBindingListView, ITypedList
   {
      #region Events

      /// <summary>
      /// Occurs when the SortableBindingList completes a sorting operation.
      /// </summary>
      public event EventHandler<SortedEventArgs> Sorted;

      /// <summary>
      /// Raises the Sorted event.
      /// </summary>
      /// <param name="e">A SortedEventArgs that contains the event data.</param>
      protected virtual void OnSorted(SortedEventArgs e)
      {
         var handler = Sorted;
         if (handler != null)
         {
            handler(this, e);
         }
      }

      /// <summary>
      /// Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged"/> event.
      /// </summary>
      /// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs"/> that contains the event data.</param>
      protected override void OnListChanged(ListChangedEventArgs e)
      {
         if (_syncObject != null && _syncObject.InvokeRequired)
         {
            _syncObject.Invoke(new Action<ListChangedEventArgs>(OnListChanged), new object[] { e });
            return;
         }

         if (e == null) return;

         // If the list is reset, check for a filter. If a filter 
         // is applied don't allow items to be added to the list.
         if (e.ListChangedType == ListChangedType.Reset)
         {
            AllowNew = String.IsNullOrEmpty(Filter);
         }
         // Add the new item to the original list.
         if (e.ListChangedType == ListChangedType.ItemAdded)
         {
            _originalList.Add(this[e.NewIndex]);
            if (!String.IsNullOrEmpty(Filter))
            {
               string cachedFilter = Filter;
               Filter = "";
               Filter = cachedFilter;
            }
         }
         // Remove the new item from the original list.
         if (e.ListChangedType == ListChangedType.ItemDeleted)
         {
            _originalList.RemoveAt(e.NewIndex);
         }

         base.OnListChanged(e);
      }

      #endregion

      #region Fields

      private PropertyDescriptorCollection _shape;
      private readonly ISynchronizeInvoke _syncObject;
      private readonly List<T> _originalList = new List<T>();

      #endregion
      
      #region Constructor

      /// <summary>
      /// Initializes a new instance of the SortableBindingList class.
      /// </summary>
      public SortableBindingList()
      {
         /* Default to non-sorted columns */
         _sortColumns = false;

         /* Get shape (only get public properties marked browsable true) */
         _shape = GetShape();
      }

      /// <summary>
      /// Initializes a new instance of the SortableBindingList class with a list of items.
      /// </summary>
      public SortableBindingList(IList<T> list)
         : base(list)
      {
         /* Default to non-sorted columns */
         _sortColumns = false;

         /* Get shape (only get public properties marked browsable true) */
         _shape = GetShape();
      }

      /// <summary>
      /// Initializes a new instance of the SortableBindingList class with a synchronization object.
      /// </summary>
      public SortableBindingList(ISynchronizeInvoke syncObject)
      {
         /* Default to non-sorted columns */
         _sortColumns = false;

         /* Get shape (only get public properties marked browsable true) */
         _shape = GetShape();

         _syncObject = syncObject;
      }

      #endregion

      #region SortableBindingList<T> Column Sorting API

      private bool _sortColumns;
      /// <summary>
      /// Gets or sets a value indicating if the columns are to be sorted.
      /// </summary>
      public bool SortColumns
      {
         get { return _sortColumns; }
         set
         {
            if (value != _sortColumns)
            {
               /* Set Column Sorting */
               _sortColumns = value;

               /* Set shape */
               _shape = GetShape();

               /* Fire MetaDataChanged */
               OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, -1));
            }
         }
      }

      private PropertyDescriptorCollection GetShape()
      {
         /* Get shape (only get public properties marked browsable true) */
         PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(typeof(T), new Attribute[] { new BrowsableAttribute(true) });

         /* Sort if required */
         if (_sortColumns)
         {
            pdc = pdc.Sort();
         }

         return pdc;
      }

      #endregion

      #region BindingList<T> Sorting Overrides

      /// <summary>
      /// Gets or sets a value indicating whether the list is sorted. 
      /// </summary>
      protected bool IsSorted { get; set; }
      /// <summary>
      /// Gets a value indicating whether the list is sorted. 
      /// </summary>
      /// <returns>true if the list is sorted; otherwise, false. The default is false.</returns>
      protected override bool IsSortedCore
      {
         get { return IsSorted; }
      }

      /// <summary>
      /// Gets the property descriptor that is used for sorting the list if sorting is implemented in a derived class; otherwise, returns null. 
      /// </summary>
      /// <returns>The <see cref="T:System.ComponentModel.PropertyDescriptor"/> used for sorting the list.</returns>
      protected override PropertyDescriptor SortPropertyCore
      {
         get { return SortComparer.Property; }
      }

      /// <summary>
      /// Gets the direction the list is sorted.
      /// </summary>
      /// <returns>One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values. The default is <see cref="F:System.ComponentModel.ListSortDirection.Ascending"/>.</returns>
      protected override ListSortDirection SortDirectionCore
      {
         get { return SortComparer.Direction; }
      }

      /// <summary>
      /// Gets a value indicating whether the list supports sorting.
      /// </summary>
      /// <returns>true if the list supports sorting; otherwise, false. The default is false.</returns>
      protected override bool SupportsSortingCore
      {
         get { return SortComparer.SupportsSorting; }
      }

      /// <summary>
      /// Sorts the items if overridden in a derived class; otherwise, throws a <see cref="T:System.NotSupportedException"/>.
      /// </summary>
      /// <param name="prop">A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that specifies the property to sort on.</param>
      /// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</param>
      protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
      {
         if (prop != null)
         {
            /* Set the sort property and direction (in the comparer) */
            SortComparer.SetSortProperties(prop, direction);

            ApplySortCoreInternal(true);
         }
      }

      /// <summary>
      /// Sorts the items and optionally fires the ListChanged event.
      /// </summary>
      /// <param name="fireListChanged">true to fire the ListChanged event; otherwise, false.</param>
      protected virtual void ApplySortCoreInternal(bool fireListChanged)
      {
         var items = Items as List<T>;
         if (items != null)
         {
            /* Execute the sort */
            items.Sort(SortComparer);

            /* Set sorted */
            IsSorted = true;

            if (SortComparer.SortMode.Equals(SortMode.Simple))
            {
               OnSorted(new SortedEventArgs(SortComparer.Property.Name, SortComparer.Direction));
            }
            else if (SortComparer.SortMode.Equals(SortMode.Advanced))
            {
               if (SortComparer.SortDescriptions.Count != 0)
               {
                  ListSortDescription sort = SortComparer.SortDescriptions[0];
                  OnSorted(new SortedEventArgs(sort.PropertyDescriptor.Name, sort.SortDirection));
               }
            }
            if (fireListChanged)
            {
               OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
         }
         else
         {
            /* Set sorted */
            IsSorted = false;
         }
      }

      /// <summary>
      /// Removes any sort applied with <see cref="M:System.ComponentModel.BindingList`1.ApplySortCore(System.ComponentModel.PropertyDescriptor,System.ComponentModel.ListSortDirection)"/> if sorting is implemented in a derived class; otherwise, raises <see cref="T:System.NotSupportedException"/>.
      /// </summary>
      protected override void RemoveSortCore()
      {
         IsSorted = false;
      }

      #endregion

      #region SortComparer<T> Public API

      private SortComparer<T> _sortComparer;
      /// <summary>
      /// User plugable sorting comparer.
      /// </summary>
      public SortComparer<T> SortComparer
      {
         get { return _sortComparer ?? (_sortComparer = new SortComparer<T>()); }
         set { _sortComparer = value; }
      }

      #endregion

      #region IBindingListView Implementation

      /// <summary>
      /// Gets the collection of sort descriptions currently applied to the data source.
      /// </summary>
      /// <returns>The <see cref="T:System.ComponentModel.ListSortDescriptionCollection"/> currently applied to the data source.</returns>
      public ListSortDescriptionCollection SortDescriptions
      {
         get { return SortComparer.SortDescriptions; }
      }

      /// <summary>
      /// Gets a value indicating whether the data source supports advanced sorting. 
      /// </summary>
      /// <returns>true if the data source supports advanced sorting; otherwise, false.</returns>
      public bool SupportsAdvancedSorting
      {
         get { return SortComparer.SupportsAdvancedSorting; }
      }

      /// <summary>
      /// Sorts the data source based on the given <see cref="T:System.ComponentModel.ListSortDescriptionCollection"/>.
      /// </summary>
      /// <param name="sorts">The <see cref="T:System.ComponentModel.ListSortDescriptionCollection"/> containing the sorts to apply to the data source.</param>
      public virtual void ApplySort(ListSortDescriptionCollection sorts)
      {
         if (sorts != null)
         {
            /* Set the sort descriptions (in the comparer) */
            SortComparer.SetSortProperties(sorts);

            ApplySortCoreInternal(true);
         }
      }

      private void ResetList()
      {
         ClearItems();
         foreach (var item in _originalList)
         {
            Add(item);
         }
         if (IsSortedCore)
         {
            ApplySortCoreInternal(true);
         }
      }

      /// <summary>
      /// Gets a value indicating whether the data source supports filtering. 
      /// </summary>
      /// <returns>true if the data source supports filtering; otherwise, false.</returns>
      public virtual bool SupportsFiltering
      {
         get { return true; }
      }

      /// <summary>
      /// Gets the error message from the last filter attempt.
      /// </summary>
      public string FilterError { get; private set; }

      private string _filter;
      /// <summary>
      /// Gets or sets the filter to be used to exclude items from the collection of items returned by the data source
      /// </summary>
      /// <returns>The string used to filter items out in the item collection returned by the data source.</returns>
      public virtual string Filter
      {
         get { return _filter; }
         set
         {
            if (_filter == value) return;

            // Turn off list-changed events.
            RaiseListChangedEvents = false;
            try
            {
               // If the value is null or empty, reset list.
               if (String.IsNullOrEmpty(value))
               {
                  ResetList();
                  // Set the filter value
                  _filter = value;
                  // Clear the filter error
                  FilterError = null;
               }
               else
               {
                  try
                  {
                     ApplyFilter(value);
                     // Set the filter value
                     _filter = value;
                     // Clear the filter error
                     FilterError = null;
                  }
                  catch (Exception ex)
                  {
                     ResetList();
                     // Set the filter value
                     _filter = null;
                     // Set the filter error
                     FilterError = ex.Message;
                  }
               }
            }
            finally 
            {
               // Turn on list changed events.
               RaiseListChangedEvents = true;
               OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
         }
      }

      private void ApplyFilter(string filter)
      {
         var results = _originalList.AsQueryable().Where(filter).ToList();

         ClearItems();
         foreach (T item in results)
         {
            Add(item);
         }
         if (IsSortedCore)
         {
            ApplySortCoreInternal(true);
         }
      }

      /// <summary>
      /// Removes the current filter applied to the data source.
      /// </summary>
      public virtual void RemoveFilter()
      {
         Filter = null;
      }

      #endregion

      #region ITypedList Implementation
      #pragma warning disable 1591
      
      public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
      {
         PropertyDescriptorCollection pdc;

         if (listAccessors != null && listAccessors.Length > 0)
         {
            // Return child list shape.
            pdc = ListBindingHelper.GetListItemProperties(listAccessors[0].PropertyType);
         }
         else
         {
            // Return properties in sort order.
            pdc = _shape;
         }

         return pdc;
      }

      // This method is only used in the design-time framework 
      // and by the obsolete DataGrid control.
      public string GetListName(PropertyDescriptor[] listAccessors)
      {
         return typeof(T).Name;
      }

      #pragma warning restore 1591
      #endregion
   }

   /// <summary>
   /// Specifies the sorting mode.
   /// </summary>
   public enum SortMode
   {
      /// <summary>
      /// No sorting.
      /// </summary>
      None,
      /// <summary>
      /// Simple, single property sorting.
      /// </summary>
      Simple,
      /// <summary>
      /// Advanced, multi property sorting.
      /// </summary>
      Advanced
   }

   /// <summary>
   /// Represents a comparer for the SortableBindingList that supports comparing one or more properties of two objects.
   /// </summary>
   /// <typeparam name="T">The type of objects to compare.</typeparam>
   public class SortComparer<T> : IComparer<T>
   {
      // The following code contains code implemented by Rockford Lhotka:
      // http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnadvnet/html/vbnet01272004.asp

      /// <summary>
      /// Gets a value indicating the active sorting mode.
      /// </summary>
      public SortMode SortMode
      {
         get
         {
            if (Property != null)
            {
               return SortMode.Simple;
            }
            if (SortDescriptions != null)
            {
               return SortMode.Advanced;
            }
            return SortMode.None;
         }
      }

      /// <summary>
      /// Gets a value indicating whether the comparer supports simple sorting.
      /// </summary>
      public virtual bool SupportsSorting
      {
         get { return true; }
      }

      /// <summary>
      /// Gets a value indicating whether the comparer supports advanced sorting.
      /// </summary>
      public virtual bool SupportsAdvancedSorting
      {
         get { return true; }
      }

      /// <summary>
      /// Gets the property descriptor used in a simple sort.
      /// </summary>
      public PropertyDescriptor Property { get; private set; }

      /// <summary>
      /// Gets the sort direction used in a simple sort.
      /// </summary>
      public ListSortDirection Direction { get; private set; }

      /// <summary>
      /// Gets the sort description collection used in an advanced sort.
      /// </summary>
      public ListSortDescriptionCollection SortDescriptions { get; private set; }

      /// <summary>
      /// Sets the sorting properties.
      /// </summary>
      /// <param name="property">Property descriptor for a simple sort.</param>
      /// <param name="direction">Sort direction for a simple sort.</param>
      public void SetSortProperties(PropertyDescriptor property, ListSortDirection direction)
      {
         Property = property;
         Direction = direction;
         SortDescriptions = null;
      }

      /// <summary>
      /// Sets the sorting properties.
      /// </summary>
      /// <param name="sortDescriptions">Sort description collection for an advanced sort.</param>
      public void SetSortProperties(ListSortDescriptionCollection sortDescriptions)
      {
         Property = null;
         SortDescriptions = sortDescriptions;
      }

      /// <summary>
      /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      public int Compare(T x, T y)
      {
         /* Knowing how to sort is dependent on what sorting properties are set */
         if (SupportsSorting && Property != null)
         {
            return CompareInternal(x, y);
         }
         if (SupportsAdvancedSorting && SortDescriptions != null)
         {
            return RecursiveCompareInternal(x, y, 0);
         }

         return 0;
      }

      #region protected virtual

      /// <summary>
      /// Single property compare method.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      protected virtual int CompareInternal(T x, T y)
      {
         /* Get property values */
         object xValue = GetPropertyValue(x, Property);
         object yValue = GetPropertyValue(y, Property);

         /* Determine sort order */
         if (Direction == ListSortDirection.Ascending)
         {
            return CompareAscending(xValue, yValue);
         }

         return CompareDescending(xValue, yValue);
      }

      /// <summary>
      /// Multiple property compare method.
      /// </summary>
      /// <param name="x">The first object to compare.</param>
      /// <param name="y">The second object to compare.</param>
      /// <param name="index">Zero based index of SortDescriptions collection.</param>
      protected virtual int RecursiveCompareInternal(T x, T y, int index)
      {
         if (index >= SortDescriptions.Count)
         {
            return 0; // termination condition
         }

         /* Get property values */
         ListSortDescription listSortDesc = SortDescriptions[index];
         object xValue = listSortDesc.PropertyDescriptor.GetValue(x);
         object yValue = listSortDesc.PropertyDescriptor.GetValue(y);

         int result;
         /* Determine sort order */
         if (listSortDesc.SortDirection == ListSortDirection.Ascending)
         {
            result = CompareAscending(xValue, yValue);
         }
         else
         {
            result = CompareDescending(xValue, yValue);
         }

         /* If the properties are equal, compare the next property */
         if (result == 0)
         {
            return RecursiveCompareInternal(x, y, ++index);
         }

         return result;
      }

      /// <summary>
      /// Compare two property values in ascending order.
      /// </summary>
      /// <param name="xValue">The first property value to compare.</param>
      /// <param name="yValue">The second property value to compare.</param>
      protected virtual int CompareAscending(object xValue, object yValue)
      {
         int result;

         if (xValue == null && yValue == null)
         {
            result = 0;
         }
         else if (xValue == null)
         {
            result = -1;
         }
         else if (yValue == null)
         {
            result = 1;
         }
         else if (xValue is IComparable)
         {
            /* If values implement IComparable */
            result = ((IComparable)xValue).CompareTo(yValue);
         }
         else if (xValue.Equals(yValue))
         {
            /* If values don't implement IComparable but are equivalent */
            result = 0;
         }
         else
         {
            /* Values don't implement IComparable and are not equivalent, so compare as string values */
            // old comparison
            //result = xValue.ToString().CompareTo(yValue.ToString());
            // use Ordinal rules instead
            result = String.CompareOrdinal(xValue.ToString(), yValue.ToString());
         }

         return result;
      }

      /// <summary>
      /// Compare two property values in descending order.
      /// </summary>
      /// <param name="xValue">The first property value to compare.</param>
      /// <param name="yValue">The second property value to compare.</param>
      protected virtual int CompareDescending(object xValue, object yValue)
      {
         /* Return result adjusted for ascending or descending sort order ie
            multiplied by 1 for ascending or -1 for descending */
         return CompareAscending(xValue, yValue) * -1;
      }

      /// <summary>
      /// Get the property value from the object.
      /// </summary>
      /// <param name="value">Object instance.</param>
      /// <param name="propertyDescriptor">The property descriptor.</param>
      /// <returns>The property value.</returns>
      protected virtual object GetPropertyValue(T value, PropertyDescriptor propertyDescriptor)
      {
         return propertyDescriptor == null ? null : propertyDescriptor.GetValue(value);
      }

      #endregion
   }

   /// <summary>
   /// Provides data for the SortableBindingList.Sorted event.
   /// </summary>
   public class SortedEventArgs : EventArgs
   {
      private readonly string _name;
      /// <summary>
      /// Gets the name of the property used to perform the sort (or the most significant in the case of an Advanced sort).
      /// </summary>
      public string Name
      {
         get { return _name; }
      }

      private readonly ListSortDirection _direction;
      /// <summary>
      /// Gets the current sort direction.
      /// </summary>
      public ListSortDirection Direction
      {
         get { return _direction; }
      }

      /// <summary>
      /// Initalizes a new instance of the SortedEventArgs class.
      /// </summary>
      /// <param name="name">The sorted property name.</param>
      /// <param name="direction">The sort direction.</param>
      public SortedEventArgs(string name, ListSortDirection direction)
      {
         _name = name;
         _direction = direction;
      }
   }
}
