import http from 'k6/http';
import { check } from 'k6';

export let options = {
    vus: 100,  // 100 users running simultaneously
    iterations: 100, // each virtual user will execute exactly one iteration
};

export default function () {
    let payload = JSON.stringify({
        cartId: "f10a7cf0-e7a4-4d87-99c8-df37a94135f6",
        offerId: "49da77ed-c140-4808-8fd6-e577a6744090",
        eventId: "bbcc7a6a-57f2-4d78-a8ba-456c02e1497e"
    });

    let headers = {
        'Content-Type': 'application/json',
        'accept': 'application/json'
    };

    let response = http.post('http://localhost:5161/orders/carts/f10a7cf0-e7a4-4d87-99c8-df37a94135f6', payload, { headers: headers });

    check(response, { 'status was 201': (r) => r.status === 201 });
}
