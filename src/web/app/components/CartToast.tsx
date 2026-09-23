"use client";

import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { selectAddedToCartNotice } from "@/lib/selectAddedToCartNotice";

// How long the message stays on screen before it fades out on its own.
const VisibleMilliseconds = 2000;

// Rendered once in the layout, so the message can never be trapped inside a product card and never
// moves anything on the page: it is positioned over the page and ignores clicks, so a shopper can
// carry on adding products while it is still showing.
export function CartToast() {
  const notice = useSelector(selectAddedToCartNotice);

  // The only thing worth remembering is which notice has already had its turn on screen. Whether
  // the message is showing is then worked out from that, instead of being a second piece of state
  // that has to be switched on every time a notice arrives.
  const [finishedNoticeNumber, setFinishedNoticeNumber] = useState(0);

  // Counted rather than named, so adding the same product twice shows the message twice and
  // restarts the timer.
  let noticeNumber = 0;
  if (notice !== null) {
    noticeNumber = notice.noticeNumber;
  }

  useEffect(
    function hideAfterAShortWhile() {
      if (noticeNumber === 0) {
        return;
      }

      const timer = setTimeout(function hide() {
        setFinishedNoticeNumber(noticeNumber);
      }, VisibleMilliseconds);

      return function cancelTimer() {
        clearTimeout(timer);
      };
    },
    [noticeNumber],
  );

  if (notice === null) {
    return null;
  }

  const isVisible = noticeNumber > finishedNoticeNumber;

  let visibilityClassName = "pointer-events-none opacity-0 translate-y-2";
  if (isVisible) {
    visibilityClassName = "pointer-events-none opacity-100 translate-y-0";
  }

  let quantityText = "";
  if (notice.quantity > 1) {
    quantityText = ` (${notice.quantity})`;
  }

  return (
    <div
      role="status"
      aria-live="polite"
      className={`fixed bottom-6 left-1/2 z-50 -translate-x-1/2 border border-gray-300 bg-white px-4 py-2 shadow-lg transition duration-200 ${visibilityClassName}`}
    >
      <span className="text-accent">&#10003;</span> Added to cart
      <span className="text-gray-700">
        {": "}
        {notice.productName}
        {quantityText}
      </span>
    </div>
  );
}
