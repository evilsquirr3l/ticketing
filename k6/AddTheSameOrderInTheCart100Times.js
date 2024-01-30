import http from 'k6/http';
import {check} from 'k6';

export let options = {
    vus: 100,  // 100 users running simultaneously
    iterations: 100, // each virtual user will execute exactly one iteration
};

export default function () {
    let cartId = "a1ec968b-d372-428d-9f7d-8951d866e04a";
    let payload = JSON.stringify({
        cartId: cartId,
        offerId: "55b9d1d3-7a44-4e9a-8c90-b1dc4e1310a7",
        eventId: "7bceba9e-d780-4ece-8486-a3d287f8d95c"
    });

    let headers = {
        'Content-Type': 'application/json',
        'accept': 'application/json'
    };

    let response = http.post(`https://localhost:7040/orders/carts/${cartId}`, payload, {headers: headers});

    check(response, {'status was 201': (r) => r.status === 201});
}
