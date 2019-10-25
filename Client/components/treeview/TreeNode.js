import React, { Component } from 'react';
import PropTypes from 'prop-types';

import './tree-node.less';

class TreeNode extends Component {
    constructor(props) {
        super(props);

        this.state = {
            collapsed: true
        }
        this.handleExpandButtonClick = this.handleExpandButtonClick.bind(this);
    }

    handleExpandButtonClick() {
        this.setState({ collapsed: !this.state.collapsed });
    }

    render() {
        const { groups, items, name } = this.props;
        const collapsed = this.state.collapsed;

        const expand = (
            <div
                className={collapsed ? 'expand-button icon-arrow-right' : 'expand-button icon-arrow-down'}
                onClick={this.handleExpandButtonClick}>
            </div>
        );

        const imageAndText = (
            <div className="image-and-text">
                <div className="image"></div>
                <div className="text">
                    <span>{name}</span>
                </div>
            </div>
        );

        let children = null;
        if (groups.length > 0) {
            children =
            <div className="tree-node-children">
                {groups.map((g, i) => (
                    <TreeNode key={i} {...g}>
                    </TreeNode>
                ))}
            </div>
        } else if (items != null && items.length > 0) {
            const { allItems, activeItem, onLeafClick } = this.context;
            children =
            <div className="tree-node-children">
                {items.map((id, i) => {
                    const item = allItems[id];
                    const className = (item == activeItem) ? 'tree-node active leaf' : 'tree-node leaf';
                    return (
                        <div key={i} className={className}>
                            <div className="tree-node-item" onClick={() => (onLeafClick && onLeafClick(item))}>
                                <div className="image-and-text">
                                    <div className="image"></div>
                                    <div className="text">
                                        <span title={item.name}>{item.name}</span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    );
                })}
            </div>
        }

        return (
            <div className={collapsed ? 'tree-node collapsed' : 'tree-node'}>
                <div className="tree-node-item">
                    {expand}
                    {imageAndText}
                </div>
                {children}
            </div>
        );
    }
}

TreeNode.propTypes = {
    groups: PropTypes.array.isRequired,
    name: PropTypes.string.isRequired,
    level: PropTypes.number.isRequired,
    items: PropTypes.array,
};

TreeNode.contextTypes = {
    allItems: PropTypes.object.isRequired,
    activeItem: PropTypes.object,
    onLeafClick: PropTypes.func
}

export default TreeNode;