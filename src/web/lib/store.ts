import { configureStore } from "@reduxjs/toolkit";
import { cartReducer } from "@/lib/cartSlice";
import type { CartState } from "@/lib/cartSlice";

// RootState is written out by hand so the shape of the store is visible: only the cart lives here.
export type RootState = {
  cart: CartState;
};

// One store per browser tab, created by StoreProvider. Follows https://redux-toolkit.js.org/usage/nextjs
export function makeStore() {
  const store = configureStore({
    reducer: {
      cart: cartReducer,
    },
  });

  return store;
}

export type AppStore = ReturnType<typeof makeStore>;

export type AppDispatch = AppStore["dispatch"];
