// This file was @generated with LibOVRPlatform/codegen/main. Do not modify it!

namespace Oculus.Platform.Models
{
  using System;
  using System.Collections;
  using Oculus.Platform.Models;
  using System.Collections.Generic;
  using UnityEngine;

  /// A purchase is made when a user buys a Product. The IAP product, which can
  /// represent a consumable item, a durable item, or a subscription, must be
  /// defined for purchase through the developer dashboard.
  public class Purchase
  {
    /// The developer payload feature is unimplemented.
    public readonly string DeveloperPayload;
    /// The time when the purchased Product expires. This value only applies to
    /// subscriptions, and will be null for durable and consumable IAP items.
    public readonly DateTime ExpirationTime;
    /// The timestamp that represents when the user was granted entitlement to the
    /// Product that was purchased.
    public readonly DateTime GrantTime;
    public readonly string ID;
    /// The Reporting ID feature is not implemented.
    public readonly string ReportingId;
    /// The SKU of the IAP Product that was purchased. This value is case-
    /// sensitive. To retrieve the product information, you can use this value when
    /// calling IAP.GetProductsBySKU().
    public readonly string Sku;


    public Purchase(IntPtr o)
    {
      DeveloperPayload = CAPI.ovr_Purchase_GetDeveloperPayload(o);
      ExpirationTime = CAPI.ovr_Purchase_GetExpirationTime(o);
      GrantTime = CAPI.ovr_Purchase_GetGrantTime(o);
      ID = CAPI.ovr_Purchase_GetPurchaseStrID(o);
      ReportingId = CAPI.ovr_Purchase_GetReportingId(o);
      Sku = CAPI.ovr_Purchase_GetSKU(o);
    }
  }

  /// Represents a paginated list of Purchase elements
  public class PurchaseList : DeserializableList<Purchase> {
    /// Instantiates a C# wrapper class that wraps a native list by pointer. Used internally by Platform SDK to wrap the list.
    public PurchaseList(IntPtr a) {
      var count = (int)CAPI.ovr_PurchaseArray_GetSize(a);
      _Data = new List<Purchase>(count);
      for (int i = 0; i < count; i++) {
        _Data.Add(new Purchase(CAPI.ovr_PurchaseArray_GetElement(a, (UIntPtr)i)));
      }

      _NextUrl = CAPI.ovr_PurchaseArray_GetNextUrl(a);
    }

  }
}