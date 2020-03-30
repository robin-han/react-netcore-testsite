import fetch from 'cross-fetch';
import "regenerator-runtime/runtime";

export const RECEIVE_TESTS = 'RECEIVE_TESTS';
export const BATCH_TEST_START = 'BATCH_TEST_START';
export const BATCH_TEST_RUNNING = 'BATCH_TEST_RUNNING';
export const BATCH_TEST_END = 'BATCH_TEST_END';
export const RUN_TEST = 'RUN_TEST';
export const REFRESH_TEST = 'REFRESH_TEST';

function _receiveTestsAction(tests) {
    return {
        type: RECEIVE_TESTS,
        tests: tests
    }
}

export function batchTestStartAction() {
    return {
        type: BATCH_TEST_START,
        result: {}
    }
}

function batchTestRunningAction(result) {
    return {
        type: BATCH_TEST_RUNNING,
        result: result
    }
}

export function batchTestEndAction() {
    return {
        type: BATCH_TEST_END,
        result: { result: 'Finish!!!' }
    }
}

function _runTestAction(result) {
    return {
        type: RUN_TEST,
        result: result
    }
}

function _refreshTestAction(result) {
    return {
        type: REFRESH_TEST,
        result: result
    }
}


export function fetchTestsAction() {
    return dispatch => {
        return fetch('http://localhost:7021/api/test', {
            method: "GET"
        }).then((response) => {
            return response.json();
        }).then((json) => {
            return dispatch(_receiveTestsAction(json));
        });
    };
}

export function runBatchTestAction(test) {
    return async dispatch => {
        let result = await fetch('http://localhost:7021/api/test/batch/' + test.id,
            {
                method: "PUT"
            })
            .then((response) => {
                return response.json();
            })
            .then((json) => {
                return json;
            });

        dispatch(batchTestRunningAction(result));
    };
}

export function runTestAction(test) {
    return async dispatch => {
        let result = await fetch('http://localhost:7021/api/test/' + test.id,
            {
                method: "PUT"
            })
            .then((response) => {
                return response.json();
            })
            .then((json) => {
                return json;
            });

        dispatch(_runTestAction(result));
    };
}

export function refreshTestAction(test) {
    return dispatch => {
        return fetch('http://localhost:7021/api/test',
            {
                method: "PUT",
                body: JSON.stringify(test),
                headers: {
                    'Content-Type': 'application/json'
                }
            })
            .then((response) => {
                return response.json();
            })
            .then((json) => {
                return dispatch(_refreshTestAction(json));
            });
    };
}
