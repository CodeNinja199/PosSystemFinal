import type { RootState } from "@/lib/store";
import type { AddedToCartNotice } from "@/lib/types/AddedToCartNotice";

// A selector: a named function that reads one part of the store. Used by CartToast.
export function selectAddedToCartNotice(
  state: RootState,
): AddedToCartNotice | null {
  return state.cart.addedNotice;
}
