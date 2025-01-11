# frozen_string_literal: true

# Licensed to the Software Freedom Conservancy (SFC) under one
# or more contributor license agreements.  See the NOTICE file
# distributed with this work for additional information
# regarding copyright ownership.  The SFC licenses this file
# to you under the Apache License, Version 2.0 (the
# "License"); you may not use this file except in compliance
# with the License.  You may obtain a copy of the License at
#
#   http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing,
# software distributed under the License is distributed on an
# "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
# KIND, either express or implied.  See the License for the
# specific language governing permissions and limitations
# under the License.

require File.expand_path('../spec_helper', __dir__)
require File.expand_path('../../../../../lib/selenium/webdriver/bidi/network/cookies', __dir__)

module Selenium
  module WebDriver
    class BiDi
      describe Cookies do
        let(:cookies) { described_class.new }

        describe '#initialize' do
          it 'initializes an empty cookies hash' do
            expect(cookies.all).to eq({})
          end
        end

        describe '#all' do
          it 'returns the underlying cookies hash' do
            cookies.add_cookie('session_id', 'abc123')
            expect(cookies.all).to eq({'session_id' => 'abc123'})
          end
        end

        describe '#add_cookie' do
          it 'adds a cookie to the internal store' do
            cookies.add_cookie('foo', 'bar')
            expect(cookies['foo']).to eq('bar')
          end

          it 'updates an existing cookie if the name already exists' do
            cookies.add_cookie('foo', 'bar')
            cookies.add_cookie('foo', 'baz')
            expect(cookies['foo']).to eq('baz')
          end
        end

        describe '#remove_cookie' do
          it 'removes a cookie by name' do
            cookies.add_cookie('foo', 'bar')
            cookies.remove_cookie('foo')
            expect(cookies['foo']).to be_nil
          end

          it 'does not raise an error if cookie does not exist' do
            expect { cookies.remove_cookie('non_existent') }.not_to raise_error
          end
        end

        describe '#[]=' do
          it 'adds a cookie using bracket assignment' do
            cookies['key1'] = 'value1'
            expect(cookies['key1']).to eq('value1')
          end
        end

        describe '#[]' do
          it 'retrieves the value of a cookie by name' do
            cookies['key2'] = 'value2'
            expect(cookies['key2']).to eq('value2')
          end

          it 'returns nil for unknown cookies' do
            expect(cookies['does_not_exist']).to be_nil
          end
        end

        describe '#delete' do
          it 'removes a cookie via bracket-based delete' do
            cookies['key3'] = 'value3'
            cookies.delete('key3')
            expect(cookies['key3']).to be_nil
          end
        end

        describe '#serialize' do
          it 'returns an array of cookie hashes in the minimal format' do
            cookies['key4'] = 'value4'
            cookies.add_cookie('session_id', 'xyz123')

            serialized = cookies.serialize
            expect(serialized).to be_an(Array)
            expect(serialized.size).to eq(2)

            key4_item = serialized.find { |h| h[:name] == 'key4' }
            expect(key4_item).not_to be_nil
            expect(key4_item[:value][:type]).to eq('string')
            expect(key4_item[:value][:value]).to eq('value4')

            session_item = serialized.find { |h| h[:name] == 'session_id' }
            expect(session_item).not_to be_nil
            expect(session_item[:value][:value]).to eq('xyz123')
          end

          it 'returns an empty array if no cookies are set' do
            expect(cookies.serialize).to eq([])
          end
        end
      end
    end
  end
end
