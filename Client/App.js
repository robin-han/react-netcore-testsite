import React, { Component } from 'react';
import PropTypes from 'prop-types';
import { connect } from 'react-redux';
import "regenerator-runtime/runtime";

import { UnControlled as CodeMirror } from 'react-codemirror2';
// The following two imports is for the theme.
import 'codemirror/lib/codemirror.css';
import 'codemirror/theme/material.css';
// This import is for the language syntax highlighting.
import 'codemirror/mode/javascript/javascript.js';

import {
    fetchTestsAction,
    runTestAction,
    refreshTestAction,
    batchTestStartAction,
    runBatchTestAction,
    batchTestEndAction,
} from './actions';

import { TestState, TestResultState } from './enums';

import TreeView from './components/treeview/TreeView';
import ResultView from './components/result-view/ResultView';
import Toolbar from './components/toolbar/Toolbar';
// import { confirmAlert } from './components/confirm/Confirm';


class App extends Component {
    constructor(props) {
        super(props);

        this.state = {
            testState: TestState.Unstarted,
            isSidebarToggled: false
        };

        this.handleToggleSidebar = this.handleToggleSidebar.bind(this);
        this.handleActiveTestChange = this.handleActiveTestChange.bind(this);
        this.handleRefreshTest = this.handleRefreshTest.bind(this);
        this.handleRunTest = this.handleRunTest.bind(this);
        // this.handleUpdateTest = this.handleUpdateTest.bind(this);

        this._codeText = '';
        this._runningIndex = -1;
    }

    handleToggleSidebar() {
        this.setState({ isSidebarToggled: !this.state.isSidebarToggled });
    }

    handleActiveTestChange(item) {
        if (this.state.testState == TestState.Running) {
            return;
        }

        const { runTest } = this.props;
        runTest(item);
    }

    //handleUpdateTest() {
    //    if (this.state.testState == TestState.Running) {
    //        return;
    //    }

    //    const { tests, result, updateTest } = this.props;
    //    const test = tests[result.testId];
    //    if (test) {
    //        confirmAlert({
    //            title: 'Update Test',
    //            message: 'Are you sure to upate test result as expected.',
    //            buttons: [
    //                {
    //                    label: 'Yes',
    //                    onClick: () => updateTest(test)
    //                },
    //                {
    //                    label: 'No',
    //                    onClick: () => null
    //                }
    //            ],
    //            closeOnClickOutside: false
    //        });
    //    }
    //}

    handleRefreshTest() {
        if (this.state.testState == TestState.Running) {
            return;
        }

        const { tests, result, refreshTest } = this.props;
        const codeText = this._codeText;
        if (codeText) {
            const test = tests[result.testId];

            let renderSize = null; // TODO: 
            const renderContainer = document.getElementsByClassName('current')[0];
            if (renderContainer && renderContainer.style.width && renderContainer.style.height) {
                renderSize = { width: parseFloat(renderContainer.style.width), height: parseFloat(renderContainer.style.height) };
            }

            refreshTest({
                id: (test && test.id),
                content: codeText,
                renderSize: renderSize
            });
        }
    }

    async handleRunTest() {
        if (this.state.testState == TestState.Running) {
            return;
        }

        this.setState({ testState: TestState.Running });
        const { tests, testIds, runBatchTest, endBatchTest } = this.props;
        while (this._runningIndex < testIds.length - 1) {
            const test = tests[++this._runningIndex];
            if (test) {
                await runBatchTest(test);

                const resultState = this.props.result.state;
                if (resultState != TestResultState.Success) {
                    this.setState({ testState: TestState.Suspended });
                    return;
                }
            }
        }

        this.setState({ testState: TestState.Stopped });
        this._runningIndex = -1;
        endBatchTest();
    }

    render() {
        const { tests, groups, result, testIds } = this.props;
        const { testState, isSidebarToggled } = this.state;
        const codeText = result.testContent;
        const currentTest = tests[result.testId];
        const resultTitle = currentTest ? `${currentTest.id}/${testIds.length}: ${currentTest.path}` : '';
        const isTestRunning = testState == TestState.Running;

        return (
            <div className={isTestRunning ? 'app test-running' : 'app'}>
                {isTestRunning
                    ? null
                    : (<aside className={"sidebar" + (isSidebarToggled ? " toggled" : "")}>
                        <TreeView
                            items={tests}
                            groups={groups}
                            onActiveItemChange={this.handleActiveTestChange}>
                        </TreeView>
                    </aside>)
                }

                {isTestRunning
                    ? null
                    : (<div className="code-view">
                        <CodeMirror
                            value={codeText}
                            options={{
                                mode: { name: 'javascript', json: true },
                                theme: 'material',
                                lineNumbers: true,
                                readOnly: false
                            }}
                            onChange={(editor, data, value) => {
                                this._codeText = value;
                            }}>
                        </CodeMirror>
                    </div>)
                }

                <div className="test-view">
                    <Toolbar
                        toggleSidebar={this.handleToggleSidebar}
                        runTest={this.handleRunTest}
                        refreshTest={this.handleRefreshTest}>
                    </Toolbar>

                    <ResultView
                        title={resultTitle}
                        {...result}>
                    </ResultView>
                </div>
            </div>
        );
    }

    componentDidMount() {
        const { fetchTests } = this.props;
        fetchTests();
    }
}

App.propTypes = {
    tests: PropTypes.object.isRequired,
    groups: PropTypes.array.isRequired,
    testIds: PropTypes.array.isRequired,
    result: PropTypes.object,

    fetchTests: PropTypes.func.isRequired,

    startBatchTest: PropTypes.func,
    runBatchTest: PropTypes.func.isRequired,
    endBatchTest: PropTypes.func.isRequired,

    runTest: PropTypes.func.isRequired,
    refreshTest: PropTypes.func.isRequired
}

function mapStateToProps(state) {
    return {
        tests: state.tests,
        groups: state.groups,
        testIds: state.testIds,
        result: state.result
    };
}

function mapDispatchToProps(dispatch) {
    return {
        fetchTests: () => {
            dispatch(fetchTestsAction());
        },
        runTest: (test) => {
            dispatch(runTestAction(test));
        },
        refreshTest: (test) => {
            dispatch(refreshTestAction(test));
        },

        startBatchTest: () => {
            dispatch(batchTestStartAction());
        },
        runBatchTest: async (test) => {
            await dispatch(runBatchTestAction(test));
        },
        endBatchTest: () => {
            dispatch(batchTestEndAction());
        }
    }
}

export default connect(mapStateToProps, mapDispatchToProps)(App);



