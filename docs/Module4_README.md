# Asynchronous ASP.NET Core APIs

The purpose of the module is to consolidate in practice the materials on ‘Async programing’ and ‘REST Architectures’
topics as well as to enhance it. This module will explain the benefits and best practices for building async APIs with
ASP.NET Core.

### What should be done

Create Async REST APIs with ASP.NET Core. Use DAL designed in prev module. The following list of the resources should be
available

- Venues
    - GET /venues
    - GET /venues/{venue_id}/sections returns all sections for venue


- Events
    - GET /events
    - GET /events/{event_id}/sections/{section_id}/seats list of seats (section_id, row_id, seat_id) with seats’
      status (id, name) and price options (id, name)

- Orders
    - GET orders/carts /{cart_id} gets list of items in a cart (cart_id is a uuid, generated and stored the client side)

- POST orders/carts/{cart_id} takes object of event_id, seat_id and price_id as a payload and adds a seat to the cart.
  Returns a cart state (with total amount) back to the caller)

- DELETE orders/carts/{cart_id}/events/{event_id}/seats/{seat_id} deletes a seat for a specific cart

- PUT orders/carts/{cart_id}/book moves all the seats in the cart to a booked state. Returns a PaymentId.

> Note1: You do not need to work out the validation of the seat status – we will do this in a later module.

- Payments
    - GET payments/{payment_id} Returns the status of a payment

- POST payments/{payment_id}/complete Updates payment status and moves all the seats related to a payment to the sold
  state.

- POST payments/{payment_id}/failed Updates payment status and moves all the seats related to a payment to the available
  state.
