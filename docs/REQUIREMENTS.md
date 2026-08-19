# Requirements

Every feature of the POS system, written as a numbered plain sentence before its code exists. Each feature lists its unhappy paths with the status code the API returns, because every unhappy path becomes a test in the testing phase. Store rules apply from the system-design phase on; before it, there is one store and no store filter.

Status codes used everywhere: 200 for a read or an update that returns data, 201 for a create, 204 for a delete or an action with no body, 400 for a request that fails validation, 401 for a missing or bad token, 403 for a logged-in user who may not do this, 404 for a record that does not exist in the caller's store, 409 for a rule conflict, 500 for anything unexpected. Every error body is `{ "message": "..." }`, including the automatic 400 from DataAnnotations, which the API reshapes into that body.

## Accounts

1. A visitor can register as a customer with a full name, an email address, a password, and the store they belong to, and receives 201 with their id, name, email, and role.
   - a. A blank or missing full name returns 400.
   - b. A blank, missing, or malformed email returns 400.
   - c. A missing password or one shorter than 8 characters returns 400.
   - d. An email that is already registered returns 409 with the message "Email is already registered."
   - e. A store id that does not exist returns 404.
2. A registered user can log in with email and password and receives 200 with a token, their full name, and their role.
   - a. A blank email or a blank password returns 400.
   - b. An unknown email returns 401 with the message "Email or password is incorrect."
   - c. A wrong password returns 401 with the same message "Email or password is incorrect." so the two cases cannot be told apart.
3. The token is a JSON Web Token signed with HS256 that carries the user id, email, role, and store id, and is valid for 24 hours.
   - a. A request without a token to a protected endpoint returns 401.
   - b. A request with an expired or tampered token returns 401.
   - c. A token signed with any algorithm other than HS256 is rejected with 401.
4. Passwords are stored as bcrypt hashes; the hash is never returned, logged, or put in a token.
5. There are three roles: Customer, Cashier, and Admin. Customers register themselves; one admin and one cashier per store are created by the seed loader from passwords in configuration.
6. A user with the wrong role for an action receives 403.
   - a. A customer calling any admin or cashier endpoint returns 403.
   - b. A cashier calling an admin-only endpoint returns 403.

## Stores

7. The system serves more than one store from one database; every user, category, product, and order belongs to exactly one store.
8. Anyone can read the list of stores (id and name) without logging in, because registration needs it.
9. The store id used by every protected endpoint comes from the token, never from the request body or the query string.
   - a. An admin of one store asking for a product, category, or order of another store receives 404, as if it did not exist; every list (products, categories, orders, customers) only ever contains the admin's own store.
10. The seed loader creates two stores when the stores table is empty, each with one admin, one cashier, a few categories, and a few products, from a JSON file.

## Categories

11. Any logged-in user can read the list of categories of their store, and one category by id.
    - a. A category id that does not exist in the caller's store returns 404.
12. An admin can add a category with a name and receives 201 with the new category.
    - a. A blank name returns 400.
    - b. A name already used in the same store returns 409.
13. An admin can edit a category's name with PUT and receives 200 with the updated category.
    - a. A blank name returns 400.
    - b. An id that does not exist in the store returns 404.
    - c. A name already used by another category in the same store returns 409.
14. An admin can delete a category and receives 204.
    - a. An id that does not exist in the store returns 404.
    - b. A category that still has products returns 409.
15. The category list of a store is cached in memory for five minutes and the cache is cleared whenever a category of that store is added, edited, or deleted.

## Products and stock

16. Any logged-in user can read the products of their store, optionally filtered by category id, and one product by id.
    - a. A product id that does not exist in the caller's store returns 404.
17. A product has a name, a price, a stock quantity, a low-stock threshold, an optional image URL, and a category.
18. An admin can add a product and receives 201 with the new product.
    - a. A blank name returns 400.
    - b. A price of zero or less returns 400.
    - c. A negative stock quantity or a negative low-stock threshold returns 400.
    - d. A category id that does not exist in the store returns 404.
19. An admin can replace a product's details with PUT and receives 200 with the updated product.
    - a. The same validation failures as adding return 400.
    - b. An id that does not exist in the store returns 404.
    - c. A category id that does not exist in the store returns 404.
20. An admin can adjust a product's stock with PATCH by sending a positive or negative change and receives 200 with the new quantity.
    - a. A change of zero returns 400.
    - b. A change that would take the stock below zero returns 409.
    - c. An id that does not exist in the store returns 404.
21. An admin can delete a product and receives 204.
    - a. An id that does not exist in the store returns 404.
    - b. A product that appears in an existing order returns 409, so old receipts keep their lines.
22. A product is low on stock when its stock quantity is below or equal to its low-stock threshold after an order takes stock away.

## Cart and checkout

23. The cart lives in the browser only; it holds product id, name, unit price, and quantity, its total is the sum of unit price times quantity, an item can be removed from it, and it is emptied after an order is placed.
24. Any logged-in user can place an order with a list of items (product id and quantity) and a payment method of Cash or Card, and receives 201 with the order, whose status starts as Placed.
    - a. An empty item list returns 400.
    - b. An item quantity of zero or less, or above 1000, returns 400.
    - c. A missing payment method, or one that is not Cash or Card, returns 400.
    - d. The same product listed twice in one order returns 400.
    - e. A product id that does not exist in the caller's store returns 404.
    - f. A quantity higher than the product's stock returns 409 with a message naming the product.
25. Placing an order saves the order and reduces the stock of every product in it in one database save, so both happen or neither does.
26. The order copies each product's name and unit price at the time of the order, so a later price change does not change old receipts.
27. The order's total is computed by the API from the copied prices and quantities, never taken from the request.
28. A cashier places an order the same way; it is recorded under the cashier's user id.
29. Only after the order is saved is the order-placed message published; a failure to publish never undoes the order.

## Orders

30. A logged-in user can read their own orders, newest first.
31. A logged-in user can read one order by id with its items, for a receipt.
    - a. An order that belongs to another customer returns 403 for a customer.
    - b. A cashier or admin can read any order of their store.
    - c. An order id that does not exist in the caller's store returns 404.
32. A cashier or admin can read all orders of their store, newest first.
33. A cashier or admin can change an order's status with PATCH to Completed or Cancelled and receives 200 with the updated order.
    - a. A status that is not Completed or Cancelled returns 400.
    - b. An order that is already Completed or Cancelled returns 409.
    - c. An order id that does not exist in the store returns 404.

## Reports and customers

34. An admin can read the sales summary of their store for today: the sum of totals of orders placed today that are not cancelled, and the count of those orders.
35. An admin can read the list of customers of their store (id, full name, email, registered date), never the password hash.

## Notifications

36. When an order is placed, a message of type `order.placed` is published for the customer with the order id and total.
37. When an order takes a product to or below its low-stock threshold, a message of type `stock.low` is published for every admin of that store, one message per admin per product.
39. A message carries only the type, the recipient user id, and a text; the Notification API stores one notification row per message and then acknowledges it.
40. A logged-in user can read their own notifications, newest first, with a read flag.
41. A logged-in user can mark one of their notifications as read with PATCH and receives 204.
    - a. A notification that belongs to another user returns 403.
    - b. A notification id that does not exist returns 404.
42. Messages are kept by RabbitMQ in a durable queue until the Notification API acknowledges them, so a message published while the Notification API is down is delivered when it starts again.
43. If RabbitMQ cannot be reached, the publisher logs the failure and returns, and the order that triggered it stays saved; on the receiving side the Notification API's consumer logs the failure and stops, and Docker restarts the Notification API until RabbitMQ is back.

