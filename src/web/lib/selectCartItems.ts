import type { RootState } from "@/lib/store";
import type { CartItem } from "@/lib/types/CartItem";

// A selector: a named function that reads one part of the store. Used by CartTable and the checkout form.
export function selectCartItems(state: RootState): CartItem[] {
  return state.cart.items;
}
