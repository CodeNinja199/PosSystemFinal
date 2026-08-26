import { createSlice } from "@reduxjs/toolkit";
import type { PayloadAction } from "@reduxjs/toolkit";
import type { CartItem } from "@/lib/types/CartItem";

export type CartState = {
  items: CartItem[];
};

const initialCartState: CartState = {
  items: [],
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

      if (existingItem === null) {
        state.items.push(action.payload);
      } else {
        existingItem.quantity = existingItem.quantity + action.payload.quantity;
      }
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
    },
  },
});

export const addItem = cartSlice.actions.addItem;

export const removeItem = cartSlice.actions.removeItem;

export const clearCart = cartSlice.actions.clearCart;

export const cartReducer = cartSlice.reducer;
