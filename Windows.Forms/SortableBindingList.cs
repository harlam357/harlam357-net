/*
 * harlam357.Net - Sortable Binding List
 * Copyright (C) 2010-2012 Ryan Harlamert (harlam357)
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
   public class SortableBindingList<T> : BindingList<T>, IBindingListView, ITypedList
   {
      #region Events

      public event EventHandler<SortedEventArgs> Sorted;

      protected virtual void OnSorted(SortedEventArgs e)
      {
         if (Sorted != null) Sorted(this, e);
      }

      /// <summary>
      /// Raises the <see cref="E:System.ComponentModel.BindingList`1.ListChanged"/> event.
      /// </summary>
      /// <param name="e">A <see cref="T:System.ComponentModel.ListChangedEventArgs"/> that contains the event data.</param>
      protected override void OnListChanged(ListChangedEventArgs e)
      {
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

         if (_syncObject == null)
         {
            base.OnListChanged(e);
         }
         else
         {
            _syncObject.Invoke(new Action<ListChangedEventArgs>(base.OnListChanged), new object[] { e });
         }
      }

      #endregion

      #region Fields

      private PropertyDescriptorCollection _shape;
      private readonly ISynchronizeInvoke _syncObject;
      private readonly List<T> _originalList = new List<T>();

      #endregion
      
      #region Constructor

      public SortableBindingList()
      {
         /* Default to non-sorted columns */
         _sortColumns = false;

         /* Get shape (only get public properties marked browsable true) */
         _shape = GetShape();
      }

      public SortableBindingList(IList<T> list)
         : base(list)
      {
         /* Default to non-sorted columns */
         _sortColumns = false;

         /* Get shape (only get public properties marked browsable true) */
         _shape = GetShape();
      }

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
      /// <param name="property">A <see cref="T:System.ComponentModel.PropertyDescriptor"/> that specifies the property to sort on.</param>
      /// <param name="direction">One of the <see cref="T:System.ComponentModel.ListSortDirection"/> values.</param>
      protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
      {
         if (property != null)
         {
            /* Set the sort property and direction (in the comparer) */
            SortComparer.SetSortProperties(property, direction);

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
         get { return _sortComparer ?? DefaultSortComparer; }
         set { _sortComparer = value; }
      }

      private SortComparer<T> _defaultSortComparer;

      private SortComparer<T> DefaultSortComparer
      {
         get { return _defaultSortComparer ?? (_defaultSortComparer = new SortComparer<T>()); }
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
            Items.Add(item);
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
                  }
                  catch (Exception ex)
                  {
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

      public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
      {
         PropertyDescriptorCollection pdc;

         if (null == listAccessors)
         {
            /* Return properties in sort order */
            pdc = _shape;
         }
         else
         {
            /* Return child list shape */
            pdc = ListBindingHelper.GetListItemProperties(listAccessors[0].PropertyType);
         }

         return pdc;
      }

      // This method is only used in the design-time framework 
      // and by the obsolete DataGrid control.
      public string GetListName(PropertyDescriptor[] listAccessors)
      {
         return typeof(T).Name;
      }

      #endregion
   }

   public enum SortMode
   {
      None,
      Simple,
      Advanced
   }

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
      /// Set the sorting properties.
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
      /// Set the sorting properties.
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
      /// <param name="xVal">The first object to compare.</param>
      /// <param name="yVal">The second object to compare.</param>
      public int Compare(T xVal, T yVal)
      {
         /* Knowing how to sort is dependent on what sorting properties are set */
         if (SupportsSorting && Property != null)
         {
            return CompareInternal(xVal, yVal);
         }
         if (SupportsAdvancedSorting && SortDescriptions != null)
         {
            return RecursiveCompareInternal(xVal, yVal, 0);
         }

         return 0;
      }

      #region protected virtual

      /// <summary>
      /// Single property compare method.
      /// </summary>
      /// <param name="xVal">The first object to compare.</param>
      /// <param name="yVal">The second object to compare.</param>
      protected virtual int CompareInternal(T xVal, T yVal)
      {
         /* Get property values */
         object xValue = GetPropertyValue(xVal, Property);
         object yValue = GetPropertyValue(yVal, Property);

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
      /// <param name="xVal">The first object to compare.</param>
      /// <param name="yVal">The second object to compare.</param>
      /// <param name="index">Zero based index of SortDescriptions collection.</param>
      protected virtual int RecursiveCompareInternal(T xVal, T yVal, int index)
      {
         if (index >= SortDescriptions.Count)
         {
            return 0; // termination condition
         }

         /* Get property values */
         ListSortDescription listSortDesc = SortDescriptions[index];
         object xValue = listSortDesc.PropertyDescriptor.GetValue(xVal);
         object yValue = listSortDesc.PropertyDescriptor.GetValue(yVal);

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
            return RecursiveCompareInternal(xVal, yVal, ++index);
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

         if (xValue is IComparable)
         {
            /* If values implement IComparer */
            result = ((IComparable)xValue).CompareTo(yValue);
         }
         else if (xValue.Equals(yValue))
         {
            /* If values don't implement IComparer but are equivalent */
            result = 0;
         }
         else
         {
            /* Values don't implement IComparer and are not equivalent, so compare as string values */
            result = xValue.ToString().CompareTo(yValue.ToString());
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
      /// <param name="property">The property descriptor.</param>
      /// <returns>The property value.</returns>
      protected virtual object GetPropertyValue(T value, PropertyDescriptor property)
      {
         return property.GetValue(value);
      }

      #endregion
   }

   public class SortedEventArgs : EventArgs
   {
      private readonly string _name;

      public string Name
      {
         get { return _name; }
      }

      private readonly ListSortDirection _direction;

      public ListSortDirection Direction
      {
         get { return _direction; }
      }

      public SortedEventArgs(string name, ListSortDirection direction)
      {
         _name = name;
         _direction = direction;
      }
   }
}
