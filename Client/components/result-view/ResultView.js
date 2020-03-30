import React, { Component } from 'react';
import PropTypes from 'prop-types';

import './result-view.less';

class ResultView extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const { title, result, expected, message } = this.props;
        if (message) {
            console.log(message);
        }

        const isSvg = (result && result.startsWith('<svg'));
        const isImage = (result && result.startsWith('data:image'));
        let currentResult = result;
        let expectedResult = expected;
        if (isSvg) {
            currentResult = (<div dangerouslySetInnerHTML={{ __html: result }}></div>);
            expectedResult = expected ? (<div dangerouslySetInnerHTML={{ __html: expected }}></div>) : '';
        } else if (isImage) {
            currentResult = (<img src={result} />);
            expectedResult = expected ? (<img src={expected} />) : '';
        }

        return (
            <div className="result-view">
                <div className="title" title={title}>
                    <span>{title}</span>
                </div>
                <div className="result">
                    <div className="current">{currentResult}</div>
                    <div className="expected">{expectedResult}</div>
                </div>
                {/* <textarea className="message" value={message}></textarea> */}
            </div>
        );
    }
}

ResultView.propTypes = {
    title: PropTypes.string,
    result: PropTypes.string,
    expected: PropTypes.string,
    message: PropTypes.string
}

export default ResultView;
