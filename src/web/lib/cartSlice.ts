import { createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import type { AddedToCartNotice } from "@/lib/types/AddedToCartNotice";
import type { CartItem } from "@/lib/types/CartItem";

export type CartState = {
  items: CartItem[];
  addedNotice: AddedToCartNotice | null;
  // Counts every add for the life of the tab, and is deliberately NOT reset by clearCart. The toast
  // remembers the number of the last notice it finished showing, and that memory lasts as long as the
  // tab does, so a count that restarted after a sale would leave the next few adds with no
  // confirmation at all.
  noticeCount: number;
};

const initialCartState: CartState = {
  items: [],
  addedNotice: null,
  noticeCount: 0,
};

// The one slice in the store. A reducer is a named function that receives the current state and an action and changes it;
// Redux Toolkit lets the code write to state directly and makes a new copy underneath. Follows https://redux-toolkit.js.org/api/createSlice
export const cartSlice = createSlice({
  name: "cart",
  initialState: initialCartState,
  reducers: {
    addItem(state, action: PayloadAction<CartItem>) {
      let existingItem: CartItem | null = null;
      for (const item of state.items) {
        if (item.productId === action.payload.productId) {
          existingItem = item;
        }
      }

      // One line per product: adding a product already in the cart raises its quantity rather than
      // making a second line, so the cart never shows the same product twice.
      if (existingItem === null) {
        state.items.push(action.payload);
      } else {
        existingItem.quantity = existingItem.quantity + action.payload.quantity;
      }

      // The notice the toast reads. The number only ever goes up, so adding the same product twice
      // counts as a change and the toast appears again.
      state.noticeCount = state.noticeCount + 1;
      state.addedNotice = {
        productName: action.payload.productName,
        quantity: action.payload.quantity,
        noticeNumber: state.noticeCount,
      };
    },
    removeItem(state, action: PayloadAction<number>) {
      const remainingItems: CartItem[] = [];
      for (const item of state.items) {
        if (item.productId !== action.payload) {
          remainingItems.push(item);
        }
      }

      state.items = remainingItems;
    },
    clearCart(state) {
      state.items = [];

      // The notice goes, so no confirmation is left over from the sale that has just been paid for.
      // noticeCount stays where it is, for the reason given on CartState.
      state.addedNotice = null;
    },
  },
});

export const addItem = cartSlice.actions.addItem;

export const removeItem = cartSlice.actions.removeItem;

export const clearCart = cartSlice.actions.clearCart;

export const cartReducer = cartSlice.reducer;
