# Ticketing Domain – Introduction

The purpose of the module is to provide you with the basics of terminology, actors, components and the typical use-cases
used in a ticketing domain.

Domain Vocabulary:

- Event – An event is an occurrence in time. In ticketing system, the event acts as a container for both venue (which
  holds inventory) and meta data about the event itself (name, venue, date, time).
- Venue – the place where an event is happening.
- Manifest – the seating arrangement applicable to a particular venue (complete seat map).
- Seat – the smallest manifest unit that can be purchased or booked. The are two seat types - specific designated place
  in a row and general admission. General admission means patrons can choose to sit anywhere within a particular
  section (for example, the dance floor)
- Offer – something you can buy and price for it. All seats in the ticketing system need to be sold through an offer. An
  offer is simply a configuration of properties that tell the system how the tickets should be sold
- Prices – Information about the ticket price levels (Adult, Child, VIP, etc.)
- Ticket – document (digital or printed), serving as evidence of admission price of some event. Tickets should include
  information about the event, the date and time of the event, venue and seats that have been paid for or booked.
- Customer – an authenticated user who searches for tickets for an event of interest to him and buys a ticket through an
  online system
- Event Manager – the user of the system who is responsible for administering the event (setting up the manifest,
  configuring the offer)

![img.png](img.png)

Use-cases Diagram:
![img_1.png](img_1.png)

Exemplary Data Model:

![img_2.png](img_2.png)

### What should be done

You have been approached by a potential customer who wants to develop a ticketing system for events. The main
functionality that should be supported by the system is shown in the use-cases diagram.

Task 1

- Draw a high-level system's components diagram (UI, Backend, Services, etc.)

Task 2

- Draw a state machine diagram for a seat state (available, booked, sold)

Task 3

- Draw a sequence diagram for buying a seat of a lowest price (includes finding available seats logic)
