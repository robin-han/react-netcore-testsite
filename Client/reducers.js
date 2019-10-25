import update from 'immutability-helper';

import {
    RECEIVE_TESTS,
    BATCH_TEST_START,
    BATCH_TEST_RUNNING,
    BATCH_TEST_END,
    UPDATE_TEST,
    RUN_TEST,
    REFRESH_TEST
} from './actions';


function rootReducer(state = { tests: {}, groups: [], testIds: [], result: {} }, action) {
    switch (action.type) {
        case RECEIVE_TESTS:
        {
            let tests = normalizeTests(action.tests);
            return update(state, { tests: { $set: tests.tests }, groups: { $set: tests.groups }, testIds: {$set : tests.testIds} });
        }
        case BATCH_TEST_START:
        case BATCH_TEST_RUNNING:
        case BATCH_TEST_END:
        {
            return update(state, { result: { $set: action.result } });
        }
        case UPDATE_TEST:
        {
            return update(state, { result: { $set: action.result } });
        }
        case RUN_TEST:
        {
            return update(state, { result: { $set: action.result } });
        }
        case REFRESH_TEST:
        {
            return update(state, { result: { $set: action.result } });
        }
        default:
        {
            return state
        }
    }
}

export default rootReducer;


function normalizeTests(testData) {
    let tests = {};
    let testIds = [];
    testData.forEach(t => {
        testIds.push(t.id);
        tests[t.id] = t;
    });
    let groups = groupTests(testData);

    return { tests, groups, testIds };
}
function groupTests(tests) {
    let groups = [];
    for (let i = 0; i < tests.length; i++) {
        let test = tests[i];
        let parent = null;
        let path = test.path.substring(0, test.path.lastIndexOf('/'));

        for (let j = 0; j < groups.length; j++) {
            let g = groups[j];
            if (g.path == path) {
                parent = g;
                break;
            } else if (path.startsWith(g.path) && path[g.path.length] == '/') {
                if (parent == null || parent.path.length < g.path.length) {
                    parent = g;
                }
            }
        }
        let subGroups = createGroups(parent, path.substring(parent ? parent.path.length : 0));
        groups.push(...subGroups);

        let group = subGroups.length > 0 ? subGroups[subGroups.length - 1] : parent;
        if (!group.items) {
            group.items = [];
        }
        group.items.push(test.id);
    }
    return groups.filter(g => g.level == 0);
}
function createGroups(parent, subPath) {
    let groups = [];
    let startLevel = parent ? parent.level + 1 : 0;
    let startPath = parent ? parent.path : '';
    let parts = subPath.split('/').filter(item => !!item);
    let group = parent;

    for (let i = 0; i < parts.length; i++) {
        let text = parts.slice(0, i + 1).join('/');
        let node = {
            name: parts[i],
            path: startPath ? (startPath + '/' + text) : text,
            level: startLevel + i,
            parent: group,
            groups: []
        };
        if (group != null) {
            group.groups.push(node);
        }
        group = node;
        groups.push(node);
    }
    return groups;
}
