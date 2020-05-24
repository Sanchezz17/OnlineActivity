import React, {Component} from 'react';
import {Stage, Layer, Line} from 'react-konva';
import {RoomHubEvents} from "../RoomConstants";
import Pallet from '../pallet/pallet';
import styles from './field.module.css';


export default class Field extends Component {
    constructor(props) {
        super(props);
        this.state = {
            lines: [{
                '#000': []
            }],
            isLoadingField: true,
            isDrawing: false,
            stageRef: null,
            isActiveUser: false,
            activeUser: null,
            hiddenWord: null,
            color: '#000',
            isPalletShow: false
        };
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_LINE, (line) => {
            this.setState({
                lines: [...this.state.lines, line]
            });
        });

        this.props.hubConnection.on(RoomHubEvents.CLEAR_FIELD, () => {
            this.setState({
                lines: []
            });
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_ROUND, (explainingPlayerName) => {
            if (explainingPlayerName === this.props.user.name) {
                this.setState({isActiveUser: true});
            }

            this.props.hubConnection.invoke(RoomHubEvents.REQUEST_WORD, this.props.roomId);
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_HIDDEN_WORD, (hiddenWord) => {
            this.setState({hiddenWord: hiddenWord})
        });

        await this.fetchLines();
    }

    fetchLines = async () => {
        const response = await fetch(`/api/fields/${this.props.roomId}`);
        const lines = await response.json();
        console.log(lines);
        this.setState({
            isLoadingField: false,
            lines: lines || []
        });
    }

    addLine = () => {
        this.props.hubConnection.invoke(RoomHubEvents.NEW_LINE,
            this.props.roomId, this.state.lines[this.state.lines.length - 1]);
    };

    handleMouseDown = () => {
        this.state.isDrawing = true;
        const lastLines = this.state.lines[this.state.lines.length - 1][this.state.color];
        const lastObj = {[this.state.color]: [...lastLines, []]};
        this.setState({
            lines: [
                ...this.state.lines.slice(0, this.state.lines.length - 1),
                lastObj
            ]
        });
    };

    handleMouseUp = () => {
        this.state.isDrawing = false;
        this.addLine();
    };

    handleMouseMove = () => {
        if (!this.state.isDrawing) {
            return;
        }
        const stage = this.state.stageRef.getStage();
        const point = stage.getPointerPosition();
        const lines = this.state.lines[this.state.lines.length - 1][this.state.color];
        let lastLine = lines[lines.length - 1] || [];
        lastLine = lastLine.concat([point.x, point.y]) || [];
        lines.splice(lines.length - 1, 1, lastLine);
        const lastObj = {[this.state.color]: lines.concat()};
        this.setState({
            lines: [
                ...this.state.lines.slice(0, this.state.lines.length - 1),
                lastObj
            ]
        });
    };

    clearField = () => {
        this.setState({
            lines: [
                { [this.state.color]: [] }
            ]
        });
        this.props.hubConnection.invoke(RoomHubEvents.CLEAR_FIELD, this.props.roomId);
    }

    changeColor = (color) => {
        this.setState({
            lines: [
                ...this.state.lines,
                { [color]: [] }
            ],
            color
        })
    }

    getDrawingLines = () => {
        if (!this.state.lines.length) {
            return [];
        }
        const drawingLines = [];
        for (const lineInfo of this.state.lines) {
            for (const [colorName, colorLines] of Object.entries(lineInfo)) {
                for (const line of colorLines) {
                    drawingLines.push({
                        color: colorName,
                        line: line,
                        strokeWidth: colorName === '#fff' ? 15 : 3
                    })  
                }
            }
        }
        return drawingLines;
    }

    onEraserClick = () => {
        this.setState({
            color: '#fff',
            lines: [
                ...this.state.lines,
                {'#fff': []}
            ]
        })
    }

    render() {
        if (typeof window === 'undefined') {
            return null;
        }

        return (
            <section className={styles.field} id='field'>
                {this.state.isActiveUser ?
                    <>
                        <section className={styles.drawerTools}>
                            <Pallet changeColor={this.changeColor}/>
                            <button className={styles.eraser}
                                    onClick={this.onEraserClick} />
                            <button
                                className={`btn btn-dark btn-sm ${styles.clear}`}
                                onClick={this.clearField}
                            >
                                Очистить
                            </button>
                            <button
                                className={`btn btn-primary btn-sm ${styles.end}`}
                            >
                                Завершить
                            </button>
                        </section>
                        <div className={styles.wordContainer}>
                            <span
                                className={`btn btn-warning btn-sm ${styles.word}`}
                            >
                                {`Загаданное слово: ${this.state.hiddenWord}`}
                            </span>
                        </div>
                    </> : ''
                }
                {this.state.isLoadingField
                    ? <div className={styles.loading}>
                        <p>Загрузка...</p>
                    </div>
                    : <Stage
                        width={document.getElementById('field').offsetWidth}
                        height=
                            {document.getElementById('field').offsetHeight -
                            (this.state.isActiveUser ? 30 : 0)}
                        onContentMousedown={this.state.isActiveUser ? this.handleMouseDown : () => {
                        }}
                        onContentMousemove={this.state.isActiveUser ? this.handleMouseMove : () => {
                        }}
                        onContentMouseup={this.state.isActiveUser ? this.handleMouseUp : () => {
                        }}
                        ref={node => {
                            this.state.stageRef = node
                        }}>
                        <Layer>
                            {this.getDrawingLines().map((drawingObj, i) => (
                                <Line key={i} points={drawingObj.line}
                                      stroke={drawingObj.color}
                                      strokeWidth={drawingObj.strokeWidth}
                                />
                            ))}
                        </Layer>
                    </Stage>
                }
            </section>
        )
    }
}
