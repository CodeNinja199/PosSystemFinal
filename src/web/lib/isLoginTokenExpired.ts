"use client";

// Reads the "exp" claim out of the token without checking its signature. The API is what verifies a
// token; this only stops the browser treating a dead one as a login.
//
// The httpOnly cookie this replaced did the same job for nothing: it was written with the token's own
// 24 hours, so the browser deleted it the moment the token died and the next page asked for a login.
// localStorage keeps whatever it is given for ever, so the check has to be spelled out here.
// Learned from: https://datatracker.ietf.org/doc/html/rfc7519#section-4.1.4
export function isLoginTokenExpired(token: string | null | undefined): boolean {
  // Nothing stored is not the same as expired: the caller already treats a missing token as no login.
  if (token === null || token === undefined) {
    return false;
  }

  const parts = token.split(".");
  if (parts.length !== 3) {
    // Not a token at all. The API would refuse it, so it counts as expired and the user logs in again.
    return true;
  }

  try {
    // The middle part is the payload, in base64url: the two characters that differ from plain base64
    // are swapped back and the padding the encoder left off is put back, because atob insists on it.
    let payloadBase64 = parts[1].replace(/-/g, "+").replace(/_/g, "/");
    while (payloadBase64.length % 4 !== 0) {
      payloadBase64 = payloadBase64 + "=";
    }

    const payload = JSON.parse(window.atob(payloadBase64));
    const expirySeconds: unknown = payload.exp;
    if (typeof expirySeconds !== "number") {
      return true;
    }

    // exp counts seconds since 1970; Date.now() counts milliseconds since the same moment.
    const nowInSeconds = Date.now() / 1000;
    return nowInSeconds >= expirySeconds;
  } catch {
    // A payload that cannot be read is one the API will refuse, so it is treated as expired.
    return true;
  }
}
