import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
    vus: 100,
    duration: '60s',
};

export default function () {
    let response = http.get('http://localhost:5161/events/7bceba9e-d780-4ece-8486-a3d287f8d95c/sections/ad55d47b-b26b-405a-8021-0b820656cee9/seats', {
        headers: {
            'accept': 'application/json'
        },
    });

    check(response, {
        'is status 200': (r) => r.status === 200,
    });

    sleep(1);
}
