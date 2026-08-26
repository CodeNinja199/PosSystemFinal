"use client";

import { useState } from "react";
import { Provider } from "react-redux";
import { makeStore } from "@/lib/store";
import type { AppStore } from "@/lib/store";

type StoreProviderProps = {
  children: React.ReactNode;
};

// Wraps the whole app in layout.tsx so any client component can reach the cart without prop drilling.
// useState with makeStore creates the store once, on the first render, and keeps it for the life of the tab.
export function StoreProvider(props: StoreProviderProps) {
  const children = props.children;
  const [store] = useState<AppStore>(makeStore);

  return <Provider store={store}>{children}</Provider>;
}
